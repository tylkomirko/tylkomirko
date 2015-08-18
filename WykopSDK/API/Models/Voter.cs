using Newtonsoft.Json;
using System.Collections.ObjectModel;
using WykopSDK.API.Models.Converters;

namespace WykopSDK.API.Models
{
    public class Voter
    {
        [JsonProperty("author")]
        public string AuthorName { get; set; }
        /*
        [JsonProperty("author_avatar")]
        public string AuthorAvatarURL { get; set; }
        [JsonProperty("author_group")]
        [JsonConverter(typeof(GroupConverter))]
        public UserGroup AuthorGroup { get; set; }
        [JsonProperty("author_sex")]
        [JsonConverter(typeof(SexEnumConverter))]
        public UserSex AuthorSex { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }*/

        /* Removed properties:
         public string author_avatar_big { get; set; }
         public string author_avatar_med { get; set; }
         public string author_avatar_lo { get; set; }
         * */
    }

    public class Vote
    {
        [JsonProperty("vote")]
        public uint VoteCount { get; set; }

        [JsonProperty("voters")]
        [JsonConverter(typeof(VotersConverter))]
        public ObservableCollection<string> Voters { get; set; }
    }
}
