using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ObservingThingy.Data;

namespace ObservingThingy.DataAccess
{
    public class HostListsRepository
    {
        private readonly Func<ApplicationDbContext> _factory;
        private readonly ILogger<HostListsRepository> _logger;

        public HostListsRepository(ILoggerFactory loggerfactory, Func<ApplicationDbContext> factory)
        {
            _factory = factory;
            _logger = loggerfactory.CreateLogger<HostListsRepository>();
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
