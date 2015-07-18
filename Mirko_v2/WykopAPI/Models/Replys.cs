using Newtonsoft.Json;
using WykopAPI.Models.Converters;
namespace WykopAPI.JSON
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
