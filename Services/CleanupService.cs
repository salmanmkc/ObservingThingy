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
    public class CleanupService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<CleanupService> _logger;

        TimeSpan loopdelay = TimeSpan.FromMinutes(15); // TODO: Needs to be changed to something more sane

        public CleanupService(IServiceProvider provider, ILoggerFactory loggerfactory)
        {
            _provider = provider;
            _logger = loggerfactory.CreateLogger<CleanupService>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Service {nameof(CleanupService)} started");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cleanup loop started");

                try
                {
                    using var scope = _provider.CreateScope();

                    var repo = scope.ServiceProvider.GetRequiredService<HostsRepository>();
                    var staterepo = scope.ServiceProvider.GetRequiredService<HostStatesRepository>();

                    await repo.Vacuum();

                    var states = staterepo.GetAll()
                        .Where(x => x.Timestamp < DateTimeOffset.Now - TimeSpan.FromHours(4));
                    staterepo.Delete(states);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fatal error occurred during cleanup");
                }


                _logger.LogInformation("Checking loop finished");

                await Task.Delay(loopdelay, stoppingToken);
            }
        }

    }
}
