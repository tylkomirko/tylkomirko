using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mirko_v2.ViewModel
{
    public enum YoutubeApp
    {
        NONE,
        TUBECAST,
        METROTUBE,
        TOIB,
        MYTUBE,
    };

    public class SettingsViewModel : ViewModelBase
    {
        private WykopAPI.UserInfo _userInfo = null;
        public WykopAPI.UserInfo UserInfo
        {
            get { return _userInfo; }
            set { Set(() => UserInfo, ref _userInfo, value); }
        }

        public bool FirstRun { get; set; }
        public bool NightMode { get; set; }
        public double FontScaleFactor { get; set; }
        public bool ShowAvatars { get; set; }
        public bool OnlyWIFIDownload { get; set; }
        public bool ShowPlus18 { get; set; }
        public bool TransparentTile { get; set; }

        public bool IsBlacklistEnabled { get; set; }
        public int HotTimeSpan { get; set; }

        public YoutubeApp YTApp { get; set; }

        public SettingsViewModel()
        {
            Windows.Storage.ApplicationData.Current.DataChanged += (s, o) => Load();
        }

        public int YTtoInt()
        {
            switch (YTApp)
            {
                case YoutubeApp.NONE:
                    return 0;
                case YoutubeApp.TUBECAST:
                    return 1;
                case YoutubeApp.METROTUBE:
                    return 2;
                case YoutubeApp.TOIB:
                    return 3;
                case YoutubeApp.MYTUBE:
                    return 4;
                default:
                    return 404;
            }
        }

        public YoutubeApp IntToYT(int id)
        {
            switch (id)
            {
                case 0:
                    return YoutubeApp.NONE;
                case 1:
                    return YoutubeApp.TUBECAST;
                case 2:
                    return YoutubeApp.METROTUBE;
                case 3:
                    return YoutubeApp.TOIB;
                case 4:
                    return YoutubeApp.MYTUBE;
                default:
                    return YoutubeApp.NONE;
            }
        }

        public void Load()
        {
            var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            var roamingValues = roamingSettings.Values;

            if (roamingValues.ContainsKey("FirstRun"))
                FirstRun = (bool)roamingValues["FirstRun"];
            else
                FirstRun = true;

            if (roamingValues.ContainsKey("NightMode"))
                NightMode = (bool)roamingValues["NightMode"];
            else
                NightMode = true;

            if (roamingValues.ContainsKey("FontScaleFactor"))
                FontScaleFactor = (double)roamingValues["FontScaleFactor"];
            else
                FontScaleFactor = 1.0;

            if (roamingValues.ContainsKey("ShowAvatars"))
                ShowAvatars = (bool)roamingValues["ShowAvatars"];
            else
                ShowAvatars = false;

            if (roamingValues.ContainsKey("OnlyWIFIDownload"))
                OnlyWIFIDownload = (bool)roamingValues["OnlyWIFIDownload"];
            else
                OnlyWIFIDownload = true;

            if (roamingValues.ContainsKey("TransparentTile"))
                TransparentTile = (bool)roamingValues["TransparentTile"];
            else
                TransparentTile = true;

            if (roamingValues.ContainsKey("IsBlacklistEnabled"))
                IsBlacklistEnabled = (bool)roamingValues["IsBlacklistEnabled"];
            else
                IsBlacklistEnabled = true;

            if (roamingValues.ContainsKey("HotTimeSpan"))
                HotTimeSpan = (int)roamingValues["HotTimeSpan"];
            else
                HotTimeSpan = 24;

            if (roamingValues.ContainsKey("ShowPlus18"))
                ShowPlus18 = (bool)roamingValues["ShowPlus18"];
            else
                ShowPlus18 = false;

            if (roamingValues.ContainsKey("YTApp"))
                YTApp = IntToYT((int)roamingValues["YTApp"]);
            else
                YTApp = YoutubeApp.NONE;

            if (roamingSettings.Containers.ContainsKey("UserInfo"))
            {
                var values = roamingSettings.Containers["UserInfo"].Values;
                UserInfo = new WykopAPI.UserInfo();

                UserInfo.AccountKey = (string)values["AccountKey"];
                UserInfo.UserKey = (string)values["UserKey"];
                UserInfo.UserName = (string)values["UserName"];

                UserInfo.IsAppRunning = (bool)values["IsAppRunning"];
                UserInfo.IsPushEnabled = (bool)values["IsPushEnabled"];

                UserInfo.AtNotificationsCount = (int)values["AtNotificationsCount"];
                UserInfo.HashtagNotificationsCount = (int)values["HashtagNotificationsCount"];
                UserInfo.PMNotificationsCount = (int)values["PMNotificationsCount"];

                UserInfo.LastToastDate = DateTime.Parse((string)values["LastToastDate"], null, System.Globalization.DateTimeStyles.RoundtripKind);
            }
            else
            {
                UserInfo = null;
            }

            App.ApiService.UserInfo = UserInfo;
        }

        public void Save()
        {
            var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            var roamingValues = roamingSettings.Values;

            roamingValues["FirstRun"] = FirstRun;
            roamingValues["NightMode"] = NightMode;
            roamingValues["FontScaleFactor"] = FontScaleFactor;
            roamingValues["ShowAvatars"] = ShowAvatars;
            roamingValues["OnlyWIFIDownload"] = OnlyWIFIDownload;
            roamingValues["TransparentTile"] = TransparentTile;

            roamingValues["IsBlacklistEnabled"] = IsBlacklistEnabled;

            roamingValues["HotTimeSpan"] = HotTimeSpan;
            roamingValues["ShowPlus18"] = ShowPlus18;

            roamingValues["YTApp"] = YTtoInt();

            if (UserInfo != null)
            {
                // FIXME
                //UserInfo.HashtagNotificationsCount = App.NotificationsViewModel.HashtagNotificationsCount;
                //UserInfo.AtNotificationsCount = App.NotificationsViewModel.AtNotificationsCount;
                //UserInfo.PMNotificationsCount = App.NotificationsViewModel.PMNotificationsCount;

                var container = roamingSettings.CreateContainer("UserInfo", Windows.Storage.ApplicationDataCreateDisposition.Always);
                var values = container.Values;

                values["AccountKey"] = UserInfo.AccountKey;
                values["UserKey"] = UserInfo.UserKey;
                values["UserName"] = UserInfo.UserName;

                values["IsAppRunning"] = UserInfo.IsAppRunning;
                values["IsPushEnabled"] = UserInfo.IsPushEnabled;

                values["AtNotificationsCount"] = UserInfo.AtNotificationsCount;
                values["HashtagNotificationsCount"] = UserInfo.HashtagNotificationsCount;
                values["PMNotificationsCount"] = UserInfo.PMNotificationsCount;

                values["LastToastDate"] = UserInfo.LastToastDate.ToString("o");
            }
        }

        public void Delete()
        {
            var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            var roamingValues = roamingSettings.Values;

            roamingSettings.DeleteContainer("UserInfo");
            roamingValues.Clear();

            UserInfo = null;
            App.ApiService.UserInfo = null;
        }
    }
}
