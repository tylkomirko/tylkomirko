using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using WykopAPI.Models.Converters;

namespace WykopAPI.Models
{
    public enum ConversationStatus
    {
        Read,
        New,
    };

    public enum MessageDirection
    {
        Sent,
        Received
    };

    [ImplementPropertyChanged]
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

        public List<PM> Messages { get; set; } // this is not returned by Wypok API
        public string LastMessage { get; set; } // this is not returned by Wypok API

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

        [JsonProperty("status")]
        [JsonConverter(typeof(ConversationStatusEnumConverter))]
        public ConversationStatus Status { get; set; }

        [JsonProperty("direction")]
        [JsonConverter(typeof(MessageDirectionEnumConverter))]
        public MessageDirection Direction { get; set; }

        [JsonProperty("embed")]
        public Embed Embed { get; set; }

        /* Removed properties:
         public string author_avatar_big { get; set; }
         public string author_avatar_med { get; set; }
         public string author_avatar_lo { get; set; }
         public string app { get; set; }
         * */
    }
}
