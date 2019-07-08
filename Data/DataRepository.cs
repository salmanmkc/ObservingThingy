using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ObservingThingy.Data
{
    public class HostsDataRepository
    {
        private readonly ILogger<HostsDataRepository> _logger;
        private readonly Func<ApplicationDbContext> _factory;


        public HostsDataRepository(ILoggerFactory loggerfactory, Func<ApplicationDbContext> factory)
        {
            _factory = factory;
            _logger = loggerfactory.CreateLogger<HostsDataRepository>();
        }

        internal async Task<List<Host>> GetAll()
        {
            using (var context = _factory())
                return await context.Hosts
                    .ToListAsync();
        }

        internal async Task<List<Host>> GetAllActiveWithStates()
        {
            using (var context = _factory())
                return await context.Hosts
                    .Where(x => x.IsValid)
                    .Include(x => x.States)
                    .ToListAsync();
        }

        internal async Task<List<Host>> GetAllActiveWithStates(int hostlistid)
        {
            using (var context = _factory())
                return await context.Set<HostListToHost>()
                    .Include(x => x.Host)
                    .ThenInclude(x => x.States)
                    .Where(x => x.HostListId == hostlistid)
                    .Where(x => x.Host.IsValid)
                    .Select(x => x.Host)
                    .ToListAsync();
        }

        internal async Task<Host> GetById(int id)
        {
            using (var context = _factory())
                return await context.Hosts
                    .SingleAsync(x => x.Id == id);
        }

        internal async Task Create(Host host)
        {
            using (var context = _factory())
            {
                await context.Hosts.AddAsync(host);
                await context.SaveChangesAsync();
            }
        }

        internal async Task Update(Host host)
        {
            using (var context = _factory())
            {
                context.Hosts.Update(host);
                await context.SaveChangesAsync();
            }
        }

        internal async Task Update(List<Host> hosts)
        {
            using (var context = _factory())
            {
                context.Hosts.UpdateRange(hosts);
                await context.SaveChangesAsync();
            }
        }

        internal async Task Delete(Host host)
        {
            using (var context = _factory())
            {
                context.Hosts.Remove(host);
                await context.SaveChangesAsync();
            }
        }

        internal async Task AddHostState(int hostid, HostState state)
        {
            using (var context = _factory())
            {
                var host = await context.Hosts
                    .Include(x => x.States)
                    .SingleAsync(x => x.Id == hostid);
                host.States.Add(state);
                context.Update(host);
                await context.SaveChangesAsync();
            }
        }
    }
    public class HostListsDataRepository
    {
        private readonly Func<ApplicationDbContext> _factory;
        private readonly ILogger<HostListsDataRepository> _logger;

        public HostListsDataRepository(ILoggerFactory loggerfactory, Func<ApplicationDbContext> factory)
        {
            _factory = factory;
            _logger = loggerfactory.CreateLogger<HostListsDataRepository>();
        }

        internal async Task<List<HostList>> GetAll()
        {
            using (var context = _factory())
                return await context.HostLists
                    .Include(x => x.HostListToHosts)
                    .ToListAsync();
        }

        internal async Task<HostList> Get(int id)
        {
            using (var context = _factory())
                return await context.HostLists
                    .Include(x => x.HostListToHosts)
                    .ThenInclude(x => x.Host)
                    .SingleAsync(x => x.Id == id);
        }

        internal async Task<List<Host>> GetHosts(int id)
        {
            using (var context = _factory())
                return (await this.Get(id))
                    .HostListToHosts
                    .Select(x => x.Host)
                    .ToList();
        }

        internal async Task Create(HostList hostlist)
        {
            using (var context = _factory())
            {
                await context.HostLists.AddAsync(hostlist);
                await context.SaveChangesAsync();
            }
        }

        internal async Task Update(HostList hostlist)
        {
            using (var context = _factory())
            {
                context.HostLists.Update(hostlist);
                await context.SaveChangesAsync();
            }
        }

        internal async Task Delete(HostList hostlist)
        {
            using (var context = _factory())
            {
                context.HostLists.Remove(hostlist);
                await context.SaveChangesAsync();
            }
        }

        internal async Task AddHostToHostList(Host host, HostList hostlist)
        {
            using (var context = _factory())
            {
                var link = new HostListToHost
                {
                    Host = host,
                    HostList = hostlist
                };
                hostlist.HostListToHosts.Add(link);
                context.HostLists.Update(hostlist);
                await context.SaveChangesAsync();
            }
        }
        internal async Task RemoveHostFromHostList(Host host, HostList hostlist)
        {
            using (var context = _factory())
            {
                var link = hostlist.HostListToHosts.Single(x => x.HostId == host.Id && x.HostListId == hostlist.Id);
                context.Set<HostListToHost>()
                    .Remove(link);
                await context.SaveChangesAsync();
            }
        }
    }
}
