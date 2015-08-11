using Newtonsoft.Json;
using WykopSDK.API.Models.Converters;

namespace WykopSDK.API.Models
{
    [JsonConverter(typeof(ErrorConverter))]
    public class Error
    {        
        public int Code { get; set; }
        public string Message { get; set; }
    }
}
