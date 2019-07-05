using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ObservingThingy.Data;

namespace ObservingThingy.Services
{
    public class HostRefreshService : BackgroundService
    {
        private readonly ILogger<HostRefreshService> _logger;
        private readonly HostsDataRepository _repo;

        public HostRefreshService(ILoggerFactory loggerfactory, HostsDataRepository repo)
        {
            _logger = loggerfactory.CreateLogger<HostRefreshService>();
            _repo = repo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Service {nameof(HostRefreshService)} started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CreateStateEntries();

                    await UpdateLastStateEntry(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fatal error occurred while checking status");
                }

                await Task.Delay(10000, stoppingToken);
            }
        }

        private async Task UpdateLastStateEntry(CancellationToken stoppingToken)
        {
            var ping = new Ping();
            var hosts = await _repo.GetAllWithStates();

            foreach (var host in hosts)
            {
                if (stoppingToken.IsCancellationRequested)
                    return;

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
                    await _repo.Update(host);
                }
            }
        }

        private async Task CreateStateEntries()
        {
            var hosts = await _repo.GetAllWithStates();

            foreach (var host in hosts)
                host.States.Add(new HostState { Host = host });

            await _repo.Update(hosts);
        }
    }
}
