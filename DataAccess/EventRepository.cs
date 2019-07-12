using System.Collections.Generic;
using System.Linq;
using ObservingThingy.Data;

namespace ObservingThingy.DataAccess
{
    public class EventRepository
    {
        private Queue<ApplicationEvent> _appevents = new Queue<ApplicationEvent>();
        int _idcounter = 1;

        internal void Enqueue(ApplicationEvent appevent)
        {
            _appevents.Enqueue(appevent);
        }

        internal ApplicationEvent Dequeue()
        {
            return _appevents.Dequeue();
        }

        internal bool HasElements { get { return _appevents.Count > 0; } }
    }
}