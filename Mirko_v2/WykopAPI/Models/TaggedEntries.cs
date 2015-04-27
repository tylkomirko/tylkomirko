using System.Collections.Generic;

namespace WykopAPI.Models
{
    public class Counters
    {
        public int total { get; set; }
        public int entries { get; set; }
        public int links { get; set; }
    }

    public class Meta
    {
        public string tag { get; set; }
        public bool is_observed { get; set; }
        public bool is_blocked { get; set; }
        public Counters counters { get; set; }
    }

    public class TaggedEntries
    {
        public Meta meta { get; set; }
        public List<Entry> items { get; set; }
    }
}
