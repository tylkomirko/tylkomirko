using Newtonsoft.Json;
using WykopAPI.Models.Converters;

namespace WykopAPI.Models
{
    public class Hashtag
    {
        [JsonProperty("tag")]
        public string HashtagName { get; set; }

        [JsonProperty("count")]
        [JsonConverter(typeof(StringToUIntConverter))]
        public uint Count { get; set; }
    }
}
