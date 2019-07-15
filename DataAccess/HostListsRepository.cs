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
                    .OrderBy(x => x.SortNumber)
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
                    HostList = hostlist,
                    SortNumber = int.MaxValue
                };
                hostlist.HostListToHosts.Add(link);
                context.HostLists.Update(hostlist);
                await context.SaveChangesAsync();
            }

            await EnsureHostOrder(hostlist.Id);
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

            await EnsureHostOrder(hostlist.Id);
        }

        internal async Task MoveHostDown(int hostid, int hostlistid)
        {
            await EnsureHostOrder(hostlistid);

            using (var context = _factory())
            {
                var links = await context.Set<HostListToHost>()
                    .Where(x => x.HostListId == hostlistid)
                    .ToListAsync();

                var host = links.Single(x => x.HostId == hostid);
                if (host.SortNumber <= 0)
                    return;
                var otherhost = links.Single(x => x.SortNumber == host.SortNumber - 1);

                host.SortNumber--;
                otherhost.SortNumber++;

                await context.SaveChangesAsync();
            }
        }
        internal async Task MoveHostUp(int hostid, int hostlistid)
        {
            await EnsureHostOrder(hostlistid);

            using (var context = _factory())
            {
                var links = await context.Set<HostListToHost>()
                    .Where(x => x.HostListId == hostlistid)
                    .ToListAsync();

                var host = links.Single(x => x.HostId == hostid);
                if (host.SortNumber >= (links.Count - 1))
                    return;
                var otherhost = links.Single(x => x.SortNumber == host.SortNumber + 1);

                host.SortNumber++;
                otherhost.SortNumber--;

                await context.SaveChangesAsync();
            }
        }

        private async Task EnsureHostOrder(int hostlistid)
        {
            using (var context = _factory())
            {
                var links = await context.Set<HostListToHost>()
                    .Where(x => x.HostListId == hostlistid)
                    .OrderBy(x => x.SortNumber)
                    .ToListAsync();

                for (int i = 0; i < links.Count; i++)
                    links[i].SortNumber = i;

                await context.SaveChangesAsync();
            }
        }
    }
}
