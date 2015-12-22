using Newtonsoft.Json;
using System;

namespace WykopSDK.API.Models.Converters
{
    class EmbedPreviewConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var previewURL = (string)reader.Value;
            previewURL = previewURL.Replace("w400gif.jpg", "w400.jpg"); // download preview image without nasty GIF logo on it.
            return previewURL;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }
}
