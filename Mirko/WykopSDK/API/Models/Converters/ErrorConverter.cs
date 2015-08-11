using Newtonsoft.Json;
using System;

namespace WykopSDK.API.Models.Converters
{
    class ErrorConverter : JsonConverter
    {
        private class errorInternal
        {
            [JsonRequired]
            public int code { get; set; }
            [JsonRequired]
            public string message { get; set; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Error);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            errorInternal errorInternal = null;

            try
            {
                errorInternal = serializer.Deserialize<errorInternal>(reader);
            }
            catch(Exception)
            {
                return null;
            }

            return new Error()
            {
                Code = errorInternal.code,
                Message = errorInternal.message,
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var error = value as Error;
            if(error != null)
            {
                var temp = new errorInternal()
                {
                    code = error.Code,
                    message = error.Message,
                };

                serializer.Serialize(writer, temp);
            }
        }
    }
}
