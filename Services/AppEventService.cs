using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ObservingThingy.Data;
using ObservingThingy.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace ObservingThingy.Services
{
    public class AppEventService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<AppEventService> _logger;

        public AppEventService(IServiceProvider provider, ILoggerFactory loggerfactory)
        {
            _provider = provider;
            _logger = loggerfactory.CreateLogger<AppEventService>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Service {nameof(AppEventService)} started");

            var eventprocessed = false;
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogTrace("Event loop started");
                eventprocessed = false;

                try
                {
                    using (var scope = _provider.CreateScope())
                    {
                        var eventrepo = scope.ServiceProvider.GetRequiredService<EventRepository>();
                        var tagrepo = scope.ServiceProvider.GetRequiredService<TagsRepository>();
                        var staterepo = scope.ServiceProvider.GetRequiredService<HostStatesRepository>();

                        eventprocessed = await LoopProcessor(eventrepo, tagrepo, staterepo);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fatal error occurred during event processing loop");
                }

                _logger.LogTrace("Event loop finished");

                if (!eventprocessed)
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        internal async Task<bool> LoopProcessor(EventRepository eventrepo, TagsRepository tagrepo, HostStatesRepository staterepo)
        {
            if (!eventrepo.HasElements)
                return false;

            var appevent = eventrepo.Dequeue();

            if (appevent == null)
                return true;

            switch (appevent)
            {
                case TagAddedEvent evt:
                    await ProcessTagAddedEvent(tagrepo, evt);
                    break;

                case TagRemovedEvent evt:
                    break;

                case HostOnlineEvent evt:
                    if (staterepo
                        .GetForHost(evt.Host.Id, 5)
                        .All(x => x.Status == HostState.StatusEnum.Online)
                        && !(await tagrepo.GetTagsForHost(evt.Host.Id)).Any(x => x.Name == "online"))
                    {
                        await tagrepo.AddTagToHost("online", evt.Host);
                        await tagrepo.RemoveTagFromHost("offline", evt.Host);
                    }
                    break;

                case HostOfflineEvent evt:
                    if (staterepo
                        .GetForHost(evt.Host.Id, 5)
                        .All(x => x.Status == HostState.StatusEnum.Offline)
                        && !(await tagrepo.GetTagsForHost(evt.Host.Id)).Any(x => x.Name == "offline"))
                    {
                        await tagrepo.AddTagToHost("offline", evt.Host);
                        await tagrepo.RemoveTagFromHost("online", evt.Host);
                    }
                    break;

                default:
                    _logger.LogError($"Unknown event type {appevent.ToString()}");
                    break;
            }

            return true;
        }

        private static async Task ProcessTagAddedEvent(TagsRepository tagrepo, TagAddedEvent evt)
        {
            switch (evt.Tag.Name)
            {
                case "step":
                    await tagrepo.RemoveTagFromHost(evt.Tag, evt.Host);
                    var tags = await tagrepo.GetTagsForHost(evt.Host.Id);

                    if (!tags.Any(x => x.Name == "prepare") && !tags.Any(x => x.Name == "restart") && !tags.Any(x => x.Name == "complete"))
                    {
                        await tagrepo.AddTagToHost("prepare", evt.Host);
                    }
                    else if (tags.Any(x => x.Name == "prepare") && !tags.Any(x => x.Name == "restart") && !tags.Any(x => x.Name == "complete"))
                    {
                        await tagrepo.RemoveTagFromHost("prepare", evt.Host);
                        await tagrepo.AddTagToHost("restart", evt.Host);
                    }
                    else if (!tags.Any(x => x.Name == "prepare") && tags.Any(x => x.Name == "restart") && !tags.Any(x => x.Name == "complete"))
                    {
                        await tagrepo.RemoveTagFromHost("restart", evt.Host);
                        await tagrepo.AddTagToHost("complete", evt.Host);
                    }
                    break;
            }
        }
    }
}
