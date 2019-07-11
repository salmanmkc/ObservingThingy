using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ObservingThingy.Data;

namespace ObservingThingy.DataAccess
{
    public class HostsRepository
    {
        private readonly ILogger<HostsRepository> _logger;
        private readonly Func<ApplicationDbContext> _factory;


        public HostsRepository(ILoggerFactory loggerfactory, Func<ApplicationDbContext> factory)
        {
            _factory = factory;
            _logger = loggerfactory.CreateLogger<HostsRepository>();
        }

        internal async Task<List<Host>> GetAll()
        {
            using (var context = _factory())
                return await context.Hosts
                    .ToListAsync();
        }

        internal async Task<List<Host>> GetAllWithStates()
        {
            using (var context = _factory())
                return await context.Hosts
                    .Include(x => x.States)
                    .ToListAsync();
        }

        internal async Task<List<Host>> GetAllActiveWithStates()
        {
            using (var context = _factory())
                return await context.Hosts
                    .Where(x => x.IsValid)
                    .Include(x => x.States)
                    .Include(x => x.TagToHosts)
                    .ThenInclude(x => x.Tag)
                    .ToListAsync();
        }

        internal async Task<List<Host>> GetAllActiveWithStates(int hostlistid)
        {
            using (var context = _factory())
                return (await context.Set<HostListToHost>()
                    .Include(x => x.Host)
                    .ThenInclude(x => x.States)
                    .Include(x => x.Host)
                    .ThenInclude(x => x.TagToHosts)
                    .ThenInclude(x => x.Tag)
                    .Where(x => x.Host.IsValid)
                    .Where(x => x.HostListId == hostlistid)
                    .ToListAsync())
                    .Select(x => x.Host)
                    .ToList();
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

        internal async Task Create(IEnumerable<Host> hosts)
        {
            using (var context = _factory())
            {
                await context.Hosts.AddRangeAsync(hosts);
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

        internal async Task Vacuum()
        {
            using (var context = _factory())
                await context.Database.ExecuteSqlRawAsync("VACUUM;");
        }
    }

    public class HostStatesRepository
    {
        List<HostState> _hoststates = new List<HostState>();
        int _idcounter = 1;

        internal void Add(HostState state)
        {
            state.Id = _idcounter++;
            _hoststates.Add(state);
        }

        internal List<HostState> GetForHost(int hostid, int count = 10)
        {
            return _hoststates
                .Where(x => x.HostId == hostid)
                .OrderByDescending(x => x.Timestamp)
                .Take(count)
                .ToList();
        }

        internal HostState Get(int id)
        {
            return _hoststates
                .Single(x => x.Id == id);
        }

        internal void Delete(HostState state)
        {
            _hoststates.Remove(state);
        }

        internal void Delete(IEnumerable<HostState> states)
        {
            foreach (var state in states)
                _hoststates.Remove(state);
        }
    }
}
