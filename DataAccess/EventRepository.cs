using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ObservingThingy.Data;

namespace ObservingThingy.DataAccess
{
    public class EventRepository
    {
        private readonly Func<ApplicationDbContext> _factory;
        private readonly ILogger<EventRepository> _logger;

        public EventRepository(ILoggerFactory loggerfactory, Func<ApplicationDbContext> factory)
        {
            _factory = factory;
            _logger = loggerfactory.CreateLogger<EventRepository>();
        }

        internal async Task Enqueue(ApplicationEvent appevent)
        {
            using (var context = _factory())
            {
                await context.ApplicationEvents.AddAsync(appevent);
                await context.SaveChangesAsync();
            }
        }

        internal async Task<ApplicationEvent> Dequeue()
        {
            using (var context = _factory())
            {
                var evt = await context.ApplicationEvents.FirstAsync();
                context.Remove(evt);
                await context.SaveChangesAsync();
                return evt;
            }
        }

        internal async Task Load(TagAddedEvent evt)
        {
            using (var context = _factory())
            {
                await context.Hosts.FindAsync(evt.HostId);
                await context.Tags.FindAsync(evt.TagId);
                await context.Entry(evt).Reference(x => x.Host).LoadAsync();
                await context.Entry(evt).Reference(x => x.Tag).LoadAsync();
            }
        }
        internal async Task Load(TagRemovedEvent evt)
        {
            using (var context = _factory())
            {
                await context.Hosts.FindAsync(evt.HostId);
                await context.Tags.FindAsync(evt.TagId);
                await context.Entry(evt).Reference(x => x.Host).LoadAsync();
                await context.Entry(evt).Reference(x => x.Tag).LoadAsync();
            }
        }

        internal bool HasElements
        {
            get
            {
                using (var context = _factory())
                    return context.ApplicationEvents.Count() > 0;
            }
        }
    }
}