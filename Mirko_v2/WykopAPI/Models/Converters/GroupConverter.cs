using Newtonsoft.Json;
using System;

namespace WykopAPI.Models.Converters
{
    class GroupConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            UserGroup g = (UserGroup)value;
            switch (g)
            {
                case UserGroup.Green:
                    writer.WriteValue(0); break;
                case UserGroup.Orange:
                    writer.WriteValue(1); break;
                case UserGroup.Maroon:
                    writer.WriteValue(2); break;
                case UserGroup.Admin:
                    writer.WriteValue(5); break;
                case UserGroup.Banned:
                    writer.WriteValue(1001); break;
                case UserGroup.Deleted:
                    writer.WriteValue(1002); break;
                case UserGroup.Client:
                    writer.WriteValue(2001); break;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumInt = Convert.ToUInt32(reader.Value);
            UserGroup? g = null;

            switch (enumInt)
            {
                case 0:
                    g = UserGroup.Green; break;
                case 1:
                    g = UserGroup.Orange; break;
                case 2:
                    g = UserGroup.Maroon; break;
                case 5:
                    g = UserGroup.Admin; break;
                case 1001:
                    g = UserGroup.Banned; break;
                case 1002:
                    g = UserGroup.Deleted; break;
                case 2001:
                    g = UserGroup.Client; break;
            }

            return g;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(int);
        }
    }
}
