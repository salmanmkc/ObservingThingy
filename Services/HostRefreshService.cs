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

            var looptimestamp = DateTimeOffset.Now;

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking hosts started");

                try
                {
                    using (var scope = _provider.CreateScope())
                    {
                        var hostsrepo = scope.ServiceProvider.GetRequiredService<HostsRepository>();
                        var staterepo = scope.ServiceProvider.GetRequiredService<HostStatesRepository>();

                        await CreateStateEntries(hostsrepo, staterepo);

                        await UpdateLastStateEntry(hostsrepo, staterepo, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fatal error occurred while checking status");
                }

                _logger.LogInformation("Checking hosts finished");

                _logger.LogInformation($"Loop timer {DateTimeOffset.Now - looptimestamp}");
                looptimestamp = DateTimeOffset.Now;

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task UpdateLastStateEntry(HostsRepository hostsrepo, HostStatesRepository staterepo, CancellationToken stoppingToken)
        {
            var hosts = await hostsrepo.GetAllActive();

            var checks = hosts.Select(x => UpdateSingleHostStateEntry(hostsrepo, staterepo, x));

            await Task.WhenAll(checks);
        }

        private async Task UpdateSingleHostStateEntry(HostsRepository hostsrepo, HostStatesRepository staterepo, Data.Host host)
        {
            _logger.LogInformation($"Checking host {host.Name} ({host.Hostname})");

            var state = staterepo.GetForHost(host.Id)
                .Last();

            var ping = new Ping();

            try
            {
                var reply = await ping.SendPingAsync(host.Hostname, 2000);

                switch (reply.Status)
                {
                    case IPStatus.Success:
                        state.Delay = (int)reply.RoundtripTime;
                        switch (reply.RoundtripTime)
                        {
                            case long n when n < 70:
                                state.Status = HostState.StatusEnum.Online;
                                break;
                            case long n when n < 300:
                                state.Status = HostState.StatusEnum.Warning;
                                break;
                            case long n when n < 2000:
                                state.Status = HostState.StatusEnum.Critical;
                                break;
                            default:
                                state.Status = HostState.StatusEnum.Error;
                                break;
                        }
                        break;

                    case IPStatus.TimedOut:
                        state.Status = HostState.StatusEnum.Offline;
                        break;

                    default:
                        _logger.LogError($"Unknown IPStatus {reply.Status} while checking {host.Hostname}");
                        state.Status = HostState.StatusEnum.Error;
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error while checking {host.Name} ({host.Hostname})");
                state.Status = HostState.StatusEnum.Error;
            }
            finally
            {
                _logger.LogTrace($"Check complete for host {host.Name} ({host.Hostname})");
            }
        }

        private async Task CreateStateEntries(HostsRepository hostsrepo, HostStatesRepository staterepo)
        {
            var states = (await hostsrepo.GetAllActive())
                .Select(x => new HostState { HostId = x.Id });

            staterepo.Add(states);
        }
    }
}
