using System.Collections.Generic;

namespace ObservingThingy.Data
{
    public class HostList : DataModel
    {
        public List<HostListToHost> HostListToHosts { get; set; }
    }

    public class HostListToHost
    {
        public int HostListId { get; set; }
        public HostList HostList { get; set; }
        public int HostId { get; set; }
        public Host Host { get; set; }
    }
}