using System;

namespace ObservingThingy.Data
{
    public class Tag : DataModel
    {
        public TagColor Color { get; set; } = TagColor.none;
        public bool IsBasic { get; set; } = false;
        public bool IsInverted { get; set; } = false;

        public string GetCssClasses()
        {
            var color = Color.ToString();
            var basic = IsBasic ? " basic" : "";
            var inverted = IsInverted ? " inverted" : "";

            return $"ui {color}{basic}{inverted} label";
        }
    }

    public enum TagColor : int
    {
        none = 0, red, orange, yellow, olive, green, teal, blue, violet, purple, pink, brown, grey, black, primary, secondary
    }

    public class TagToHost
    {
        public Tag Tag { get; set; }
        public int TagId { get; set; }
        public Host Host { get; set; }
        public int HostId { get; set; }
    }
}
