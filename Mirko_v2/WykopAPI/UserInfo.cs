using System;

namespace WykopAPI
{
    public class UserInfo
    {
        public bool IsAppRunning { get; set; }

        public string UserKey { get; set; }
        public string AccountKey { get; set; }
        public string UserName { get; set; }
        public DateTime LastToastDate { get; set; }

        public int HashtagNotificationsCount { get; set; }
        public int AtNotificationsCount { get; set; }
        public int PMNotificationsCount { get; set; }
    }
}
