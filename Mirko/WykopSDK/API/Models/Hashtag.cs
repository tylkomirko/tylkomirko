using Newtonsoft.Json;
using WykopSDK.API.Models.Converters;

namespace WykopSDK.API.Models
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
