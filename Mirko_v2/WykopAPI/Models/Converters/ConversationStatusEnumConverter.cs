using Newtonsoft.Json;
using System;

namespace WykopAPI.Models.Converters
{
    class ConversationStatusEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumString = reader.Value as string;
            if (enumString == null) return null;

            return enumString == "new" ? ConversationStatus.New : ConversationStatus.Read;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var s = (ConversationStatus)value;
            if(s == ConversationStatus.Read)
                writer.WriteValue("read");
            else
                writer.WriteValue("new");
        }
    }
}
