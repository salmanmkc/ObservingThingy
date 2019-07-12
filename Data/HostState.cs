using System;

namespace ObservingThingy.Data
{
    public class HostState
    {
        public int Id { get; set; }

        public int HostId { get; set; }

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;

        public int Delay { get; set; } = 0;

        public StatusEnum Status { get; set; } = StatusEnum.Unchecked;

        public enum StatusEnum : int
        {
            Unchecked = 0,
            Online,
            Warning,
            Critical,
            Offline,
            Error
        }

        public override string ToString()
        {
            return $"{Status} - {Delay}ms - {Timestamp.ToString("g")}";
        }
    }
}
