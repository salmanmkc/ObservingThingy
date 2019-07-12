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

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Event loop started");

                try
                {
                    using (var scope = _provider.CreateScope())
                    {
                        var eventrepo = scope.ServiceProvider.GetRequiredService<EventRepository>();
                        var tagrepo = scope.ServiceProvider.GetRequiredService<TagsRepository>();

                        await LoopProcessor(eventrepo, tagrepo);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fatal error occurred during event processing loop");
                }

                _logger.LogInformation("Event loop finished");

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        internal async Task LoopProcessor(EventRepository eventrepo, TagsRepository tagrepo)
        {
            if (!eventrepo.HasElements)
                return;

            var appevent = eventrepo.Dequeue();

            switch (appevent)
            {
                case TagAddedEvent evt:
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
                    break;

                case TagRemovedEvent evt:
                    break;

                default:
                    _logger.LogError($"Unknown event type {appevent.ToString()}");
                    break;
            }
        }
    }
}
