using Newtonsoft.Json;
using WykopSDK.API.Models.Converters;

namespace WykopSDK.API.Models
{
    public class EmptyReply
    {

    }

    public class EntryIDReply
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(StringToUIntConverter))]
        public uint ID { get; set; }
    }

    public class UserFavorite
    {
        public bool user_favorite { get; set; }
    }
}
