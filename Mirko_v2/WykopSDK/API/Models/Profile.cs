using Newtonsoft.Json;
using PropertyChanged;
using System;
using WykopSDK.API.Models.Converters;

namespace WykopSDK.API.Models
{
    [ImplementPropertyChanged]
    public class Profile
    {
        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("author_group")]
        [JsonConverter(typeof(GroupConverter))]
        public UserGroup Group { get; set; }
        [JsonProperty("sex")]
        [JsonConverter(typeof(SexEnumConverter))]
        public UserSex Sex { get; set; }
        [JsonProperty("avatar_big")]
        public string AvatarURL { get; set; }

        [JsonProperty("signup_date")]
        public DateTime SignupDate { get; set; }

        [JsonProperty("about")]
        public string About { get; set; }

        [JsonProperty("entries")]
        public int EntriesCount { get; set; }
        [JsonProperty("entries_comments")]
        public int CommentsCount { get; set; }
        [JsonProperty("followers")]
        public int FollowersCount { get; set; }

        [JsonProperty("is_observed")]
        public bool? Observed { get; set; }
        [JsonProperty("is_blocked")]
        public bool? Blacklisted { get; set; }

        /* Unused properties:
        public string name { get; set; }
        public string email { get; set; }
        public string public_email { get; set; }
        public string www { get; set; }
        public string jabber { get; set; }
        public string gg { get; set; }
        public string city { get; set; }
        public int links_added { get; set; }
        public int links_published { get; set; }
        public int diggs { get; set; }
        public object buries { get; set; }
        public int groups { get; set; }
        public int related_links { get; set; }
        public string url { get; set; }
        public string violation_url { get; set; }
        public string avatar { get; set; }
        public string avatar_med { get; set; }
        public string avatar_lo { get; set; }
        public int comments { get; set; }
        public int rank { get; set; }
        public int following { get; set; }
         * */
    }
}
