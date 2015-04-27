using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace WykopAPI.Models
{
    public class Voter
    {
        [JsonProperty("author")]
        public string AuthorName { get; set; }
        [JsonProperty("author_avatar")]
        public string AuthorAvatarURL { get; set; }
        [JsonProperty("author_group")]
        [JsonConverter(typeof(GroupConverter))]
        public UserGroup AuthorGroup { get; set; }
        [JsonProperty("author_sex")]
        [JsonConverter(typeof(SexEnumConverter))]
        public UserSex AuthorSex { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        /* Removed properties:
         public string author_avatar_big { get; set; }
         public string author_avatar_med { get; set; }
         public string author_avatar_lo { get; set; }
         * */
    }

    public class Vote
    {
        public int vote { get; set; } // the fuck is this?

        [JsonProperty("voters")]
        public List<Voter> Voters { get; set; }
    }
}
