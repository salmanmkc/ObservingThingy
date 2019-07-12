using System;
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

                switch (evt)
                {
                    case TagEvent e:
                        await context.Entry(e).Reference(x => x.Host).LoadAsync();
                        await context.Entry(e).Reference(x => x.Tag).LoadAsync();
                        break;

                    case HostEvent e:
                        await context.Entry(e).Reference(x => x.Host).LoadAsync();
                        break;
                }
                
                context.Remove(evt);
                await context.SaveChangesAsync();
                return evt;
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