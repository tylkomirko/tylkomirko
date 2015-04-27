namespace WykopAPI.JSON
{
    public class Profile
    {
        public string login { get; set; }
        public string email { get; set; }
        public string public_email { get; set; }
        public string name { get; set; }
        public string www { get; set; }
        public string jabber { get; set; }
        public string gg { get; set; }
        public string city { get; set; }
        public string about { get; set; }
        public int author_group { get; set; }
        public int links_added { get; set; }
        public int links_published { get; set; }
        public int comments { get; set; }
        public int rank { get; set; }
        public int followers { get; set; }
        public int following { get; set; }
        public int entries { get; set; }
        public int entries_comments { get; set; }
        public int diggs { get; set; }
        public object buries { get; set; }
        public int groups { get; set; }
        public int related_links { get; set; }
        public string signup_date { get; set; }
        public string avatar { get; set; }
        public string avatar_big { get; set; }
        public string avatar_med { get; set; }
        public string avatar_lo { get; set; }
        public bool? is_observed { get; set; }
        public bool? is_blocked { get; set; }
        public string sex { get; set; }
        public string url { get; set; }
        public string violation_url { get; set; }
    }
}
