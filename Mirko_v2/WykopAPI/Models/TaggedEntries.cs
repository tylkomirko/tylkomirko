using Newtonsoft.Json;
using System.Collections.Generic;

namespace WykopAPI.Models
{
    public class Counters
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("entries")]
        public int Entries { get; set; }

        [JsonProperty("links")]
        public int Links { get; set; }
    }

    public class Meta
    {
        [JsonProperty("tag")]
        public string Hashtag { get; set; }

        [JsonProperty("is_observed")]
        public bool Observed { get; set; }

        [JsonProperty("is_blocked")]
        public bool Blacklisted { get; set; }

        [JsonProperty("counters")]
        public Counters Counters { get; set; }
    }

    public class TaggedEntries
    {
        [JsonProperty("meta")]
        public Meta Meta { get; set; }

        [JsonProperty("items")]
        public List<Entry> Entries { get; set; }
    }
}
