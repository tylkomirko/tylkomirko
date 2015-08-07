using Newtonsoft.Json;

namespace WykopSDK.WWW
{
    public class ConnectAccountReply
    {
        [JsonProperty("appkey")]
        public string AppKey { get; set; }

        [JsonProperty("login")]
        public string Username { get; set; }

        [JsonProperty("token")]
        public string AccountKey { get; set; }

        [JsonProperty("sign")]
        public string Sign { get; set; }
    }
}
