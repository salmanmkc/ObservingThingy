using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ObservingThingy.Data;

namespace ObservingThingy.Services
{
    public class HostRefreshService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<HostRefreshService> _logger;

        public HostRefreshService(IServiceProvider provider, ILoggerFactory loggerfactory)
        {
            _provider = provider;
            _logger = loggerfactory.CreateLogger<HostRefreshService>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Service {nameof(HostRefreshService)} started");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking hosts started");

                try
                {
                    using (var scope = _provider.CreateScope())
                    {
                        var hostsrepo = scope.ServiceProvider.GetRequiredService<HostsDataRepository>();

                        await CreateStateEntries(hostsrepo);

                        await UpdateLastStateEntry(hostsrepo, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fatal error occurred while checking status");
                }

                _logger.LogInformation("Checking hosts finished");

                await Task.Delay(10000, stoppingToken);
            }
        }

        private async Task UpdateLastStateEntry(HostsDataRepository hostsrepo, CancellationToken stoppingToken)
        {
            var ping = new Ping();
            var hosts = await hostsrepo.GetAllWithStates();

            foreach (var host in hosts)
            {
                if (stoppingToken.IsCancellationRequested)
                    return;

                _logger.LogInformation($"Checking host {host.Name} ({host.Hostname})");

                var state = host.States.Last();

                try
                {
                    var reply = await ping.SendPingAsync(host.Hostname, 2000);

                    state.Delay = (int)reply.RoundtripTime;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error while checking {host.Name} ({host.Hostname})");
                    state.IsError = true;
                }
                finally
                {
                    state.IsChecked = true;
                    await hostsrepo.Update(host);
                }
            }
        }

        private async Task CreateStateEntries(HostsDataRepository hostsrepo)
        {
            var hosts = await hostsrepo.GetAllWithStates();

            foreach (var host in hosts)
                host.States.Add(new HostState { Host = host });

            await hostsrepo.Update(hosts);
        }
    }
}
