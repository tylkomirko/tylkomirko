using Newtonsoft.Json;
using WykopSDK.API.Models.Converters;

namespace WykopSDK.API.Models
{
    public enum EmbedType
    {
        Image,
        Video,
    };

    public class Embed
    {
        [JsonProperty("type")]
        [JsonConverter(typeof(EmbedTypeConverter))]
        public EmbedType Type { get; set; }
        [JsonProperty("preview")]
        public string PreviewURL { get; set; }
        [JsonProperty("url")]
        public string URL { get; set; }
        [JsonProperty("plus18")]
        public bool NSFW { get; set; }
        [JsonProperty("source")]
        public string Source { get; set; }
    }
}
