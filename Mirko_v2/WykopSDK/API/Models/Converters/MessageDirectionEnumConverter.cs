using Newtonsoft.Json;
using System;

namespace WykopSDK.API.Models.Converters
{
    class MessageDirectionEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumString = reader.Value as string;
            if (enumString == null) return null;

            return enumString == "sended" ? MessageDirection.Sent : MessageDirection.Received;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var d = (MessageDirection)value;
            if (d == MessageDirection.Sent)
                writer.WriteValue("sended");
            else
                writer.WriteValue("received");
        }
    }
}
