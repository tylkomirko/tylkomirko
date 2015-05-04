using Newtonsoft.Json;
using System;

namespace WykopAPI.Models
{
    public enum ConversationStatus
    {
        Read,
        New,
    };

    public class Conversation
    {
        [JsonProperty("last_update")]
        public DateTime LastUpdate { get; set; }

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

        [JsonProperty("status")]
        [JsonConverter(typeof(ConversationStatusEnumConverter))]
        public ConversationStatus Status { get; set; }

        /* Removed properties:
         public string author_avatar_big { get; set; }
         public string author_avatar_med { get; set; }
         public string author_avatar_lo { get; set; }
         * */
    }

    public class PM
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

        [JsonProperty("body")]
        public string Text { get; set; }

        public string status { get; set; }
        public string direction { get; set; }
        public Embed embed { get; set; }

        /* Removed properties:
         public string author_avatar_big { get; set; }
         public string author_avatar_med { get; set; }
         public string author_avatar_lo { get; set; }
         public string app { get; set; }
         * */
    }
}
