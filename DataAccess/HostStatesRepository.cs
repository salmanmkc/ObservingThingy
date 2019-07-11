using System.Collections.Generic;
using System.Linq;
using ObservingThingy.Data;

namespace ObservingThingy.DataAccess
{
    public class HostStatesRepository
    {
        List<HostState> _hoststates = new List<HostState>();
        int _idcounter = 1;

        internal void Add(HostState state)
        {
            state.Id = _idcounter++;
            _hoststates.Add(state);
        }

        internal void Add(IEnumerable<HostState> states)
        {
            foreach (var state in states)
                state.Id = _idcounter++;

            _hoststates.AddRange(states);
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
