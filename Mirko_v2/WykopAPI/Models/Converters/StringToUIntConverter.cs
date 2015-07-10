using Newtonsoft.Json;
using System;

namespace WykopAPI.Models.Converters
{
    class StringToUIntConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var str = reader.Value as string;
            return Convert.ToUInt32(str);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var i = (uint)value;
            writer.WriteValue(i.ToString());
        }
    }
}
