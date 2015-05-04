using Newtonsoft.Json;
using System;

namespace WykopAPI.Models
{
    public class ConversationStatusEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumString = (string)reader.Value;
            ConversationStatus? s = null;

            if (enumString == "new")
                s = ConversationStatus.New;
            else
                s = ConversationStatus.Read;

            return s;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            ConversationStatus s = (ConversationStatus)value;
            switch (s)
            {
                case ConversationStatus.Read:
                    writer.WriteValue("read");
                    break;

                case ConversationStatus.New:
                    writer.WriteValue("new");
                    break;
            }
        }
    }
}
