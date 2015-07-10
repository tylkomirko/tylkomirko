using Newtonsoft.Json;
using System;

namespace WykopAPI.Models.Converters
{
    class EmbedTypeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            EmbedType t = (EmbedType)value;
            switch (t)
            {
                case EmbedType.Image:
                    writer.WriteValue("image");
                    break;

                case EmbedType.Video:
                    writer.WriteValue("video");
                    break;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumString = (string)reader.Value;
            EmbedType? t = null;

            // according to docs, this can only have two values
            if (enumString == "image")
                t = EmbedType.Image;
            else
                t = EmbedType.Video;

            return t;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}
