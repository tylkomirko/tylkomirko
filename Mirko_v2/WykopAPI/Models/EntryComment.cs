using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WykopAPI.Models
{
    [ImplementPropertyChanged]
    public class EntryComment
    {
        [JsonProperty("id")]
        public uint ID { get; set; }
        [JsonProperty("entry_id")]
        public uint EntryID { get; set; }
        [JsonProperty("date")]
        public DateTime Date { get; set; }
        [JsonProperty("blocked")]
        public bool Blacklisted { get; set; }
        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

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

        [JsonProperty("body")]
        public string Text { get; set; }
        [JsonProperty("embed")]
        public Embed Embed { get; set; }

        [JsonProperty("vote_count")]
        public uint VoteCount { get; set; }
        [JsonProperty("voters")]
        public List<Voter> Voters { get; set; }
        [JsonProperty("user_vote")]
        [JsonConverter(typeof(BoolConverter))]
        public bool Voted { get; set; }

        /* Removed properties:
         public string author_avatar_big { get; set; }
         public string author_avatar_med { get; set; }
         public string author_avatar_lo { get; set; }
         public object source { get; set; }
         public string type { get; set; }
         public string app { get; set; }
         public string violation_url { get; set; }
         */
    }
}
