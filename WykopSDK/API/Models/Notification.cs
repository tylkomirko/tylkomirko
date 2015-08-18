using Newtonsoft.Json;
using System;
using WykopSDK.API.Models.Converters;

namespace WykopSDK.API.Models
{
    public enum NotificationType
    {
        Register,
        Observe,
        Unobserve,
        CommentDirected,
        EntryDirected,
        TaggedEntry,
        LinkCommentDirected,
        LinkPromoted,
        LinkDirected,
        System,
        Badge,
        SupportAnswer,
        ChannelRequest,
        ChannelAccepted,
        ChannelRejected,
        PM,
    };

    public class NotificationsCount
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class NotificationEntry
    {
        [JsonProperty("id")]
        public uint ID { get; set; }

        [JsonProperty("body")]
        public string Text { get; set; }

        /* Removed properties:
        public string url { get; set; }
         * */
    }

    public class NotificationComment
    {
        [JsonProperty("id")]
        public uint CommentID { get; set; }

        [JsonProperty("body")]
        public string Text { get; set; }
    }

    public class Notification
    {
        [JsonProperty("id")]
        public uint ID { get; set; }
        [JsonProperty("date")]
        public DateTime Date { get; set; }
        [JsonProperty("new")]
        public bool IsNew { get; set; }
        [JsonProperty("type")]
        [JsonConverter(typeof(NotificationTypeConverter))]
        public NotificationType Type { get; set; }

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

        [JsonProperty("entry")]
        public NotificationEntry Entry { get; set; }
        [JsonProperty("comment")]
        public NotificationComment Comment { get; set; }

        /* Removed properties:
        public string author_avatar_big { get; set; }
        public string author_avatar_med { get; set; }
        public string author_avatar_lo { get; set; }
        public string url { get; set; }
         * */
    }
}
