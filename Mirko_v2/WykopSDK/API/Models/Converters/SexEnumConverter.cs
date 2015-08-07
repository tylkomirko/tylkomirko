using Newtonsoft.Json;
using System;

namespace WykopSDK.API.Models.Converters
{
    class SexEnumConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            UserSex s = (UserSex)value;
            switch (s)
            {
                case UserSex.Male:
                    writer.WriteValue("male");
                    break;

                case UserSex.Female:
                    writer.WriteValue("female");
                    break;

                case UserSex.None:
                    writer.WriteValue("");
                    break;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumString = (string)reader.Value;
            UserSex? s = null;

            if (enumString == "male")
                s = UserSex.Male;
            else if (enumString == "female")
                s = UserSex.Female;
            else
                s = UserSex.None;

            return s;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}
