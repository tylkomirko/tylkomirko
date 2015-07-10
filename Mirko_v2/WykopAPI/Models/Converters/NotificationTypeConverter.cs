using Newtonsoft.Json;
using System;

namespace WykopAPI.Models.Converters
{
    class NotificationTypeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            NotificationType t = (NotificationType)value;
            switch (t)
            {
                case NotificationType.Register:
                    writer.WriteValue("register"); break;

                case NotificationType.Observe:
                    writer.WriteValue("observe"); break;
                case NotificationType.Unobserve:
                    writer.WriteValue("unobserve"); break;

                case NotificationType.EntryDirected:
                    writer.WriteValue("entry_directed"); break;
                case NotificationType.CommentDirected:
                    writer.WriteValue("entry_comment_directed"); break;
                case NotificationType.TaggedEntry:
                    writer.WriteValue("entry_tag"); break;

                case NotificationType.LinkCommentDirected:
                    writer.WriteValue("link_comment_directed"); break;
                case NotificationType.LinkDirected:
                    writer.WriteValue("link_directed"); break;
                case NotificationType.LinkPromoted:
                    writer.WriteValue("link_promoted"); break;

                case NotificationType.System:
                    writer.WriteValue("system"); break;
                case NotificationType.Badge:
                    writer.WriteValue("badge"); break;
                case NotificationType.SupportAnswer:
                    writer.WriteValue("support_answer"); break;

                case NotificationType.ChannelRequest:
                    writer.WriteValue("channel_request"); break;
                case NotificationType.ChannelAccepted:
                    writer.WriteValue("channel_accepted"); break;
                case NotificationType.ChannelRejected:
                    writer.WriteValue("channel_rejected"); break;

                case NotificationType.PM:
                    writer.WriteValue("pm"); break;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumString = (string)reader.Value;
            NotificationType? t = null;

            switch(enumString)
            {
                case "register":
                    t = NotificationType.Register; break;

                case "observe":
                    t = NotificationType.Observe; break;
                case "unobserve":
                    t = NotificationType.Unobserve; break;

                case "entry_comment_directed":
                    t = NotificationType.CommentDirected; break;
                case "entry_directed":
                    t = NotificationType.EntryDirected; break;
                case "entry_tag":
                    t = NotificationType.TaggedEntry; break;

                case "link_comment_directed":
                    t = NotificationType.LinkCommentDirected; break;
                case "link_promoted":
                    t = NotificationType.LinkPromoted; break;
                case "link_directed":
                    t = NotificationType.LinkDirected; break;

                case "system":
                    t = NotificationType.System; break;
                case "badge":
                    t = NotificationType.Badge; break;
                case "support_answer":
                    t = NotificationType.SupportAnswer; break;

                case "channel_request":
                    t = NotificationType.ChannelRequest; break;
                case "channel_accepted":
                    t = NotificationType.ChannelAccepted; break;
                case "channel_rejected":
                    t = NotificationType.ChannelRejected; break;

                case "pm":
                    t = NotificationType.PM; break;
            }
            
            return t;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}
