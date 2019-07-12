namespace ObservingThingy.Data
{
    public class ApplicationEvent : DataModel
    {
    }

    public class TagEvent : ApplicationEvent
    {
        public Host Host { get; set; }
        public Tag Tag { get; set; }
    }

    public class TagAddedEvent : TagEvent
    {

    }

    public class TagRemovedEvent : TagEvent
    {

    }
}
