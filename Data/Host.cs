using System.Collections.Generic;

namespace ObservingThingy.Data
{
    public class Host : DataModel
    {
        public string Hostname { get; set; }

        public List<HostListToHost> HostListToHosts { get; set; }
        public List<TagToHost> TagToHosts { get; set; }

        public string Comment { get; set; }
    }
}
