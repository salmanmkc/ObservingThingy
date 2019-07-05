using System;
using Microsoft.Extensions.Logging;

namespace ObservingThingy.Data
{
    public class DataRepository
    {
        private readonly ILogger<DataRepository> _logger;
        private readonly Func<ApplicationDbContext> _factory;

        public DataRepository(ILoggerFactory loggerfactory, Func<ApplicationDbContext> factory)
        {
            _factory = factory;
            _logger = loggerfactory.CreateLogger<DataRepository>();
        }
    }
}
