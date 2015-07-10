using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WykopAPI.Models.Converters
{
    class VotersConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ObservableCollection<string>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var voters = new List<string>();

            if(reader.TokenType == JsonToken.StartArray)
            {
                var fullVoters = serializer.Deserialize<List<Voter>>(reader);
                voters.AddRange(fullVoters.Select(x => x.AuthorName));
            }

            return new ObservableCollection<string>(voters);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var voters = value as ObservableCollection<string>;
            var fullVoters = new List<Voter>();
            foreach (var voter in voters)
                fullVoters.Add(new Voter() { AuthorName = voter });

            serializer.Serialize(writer, fullVoters);
        }
    }
}
