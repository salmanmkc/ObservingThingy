namespace ObservingThingy.Data
{
    public class ApplicationEvent : DataModel
    {
    }


    public class TagEvent : ApplicationEvent
    {
        public Host Host { get; set; }
        public int HostId {get;set;}
        public Tag Tag { get; set; }
        public int TagId {get;set;}
    }

    public class TagAddedEvent : TagEvent { }

    public class TagRemovedEvent : TagEvent { }


    public class HostEvent : ApplicationEvent
    {
        public Host Host { get; set; }
        public int HostId { get; set; }
    }

    public class HostStatusEvent : HostEvent { }

    public class HostOnlineEvent : HostStatusEvent { }

    public class HostOfflineEvent : HostStatusEvent { }
}
