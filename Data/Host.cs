using System;
using System.Collections.Generic;

namespace ObservingThingy.Data
{
    public class Host : DataModel
    {
        public string Hostname { get; set; }
        public List<HostState> States { get; set; }

        public List<Service> Services { get; set; }

        public string Comment { get; set; }
    }

    public class HostState
    {
        public int Id { get; set; }

        public Host Host { get; set; }
        public int HostId { get; set; }

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;

        public int Delay { get; set; }
    }
}
