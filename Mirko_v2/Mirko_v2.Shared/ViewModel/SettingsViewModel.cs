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
        public WykopAPI.UserInfo UserInfo { get; set; }

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
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var localValues = localSettings.Values;

            if (localValues.ContainsKey("FirstRun"))
                FirstRun = (bool)localValues["FirstRun"];
            else
                FirstRun = true;

            if (localValues.ContainsKey("NightMode"))
                NightMode = (bool)localValues["NightMode"];
            else
                NightMode = true;

            if (localValues.ContainsKey("FontScaleFactor"))
                FontScaleFactor = (double)localValues["FontScaleFactor"];
            else
                FontScaleFactor = 1.0;

            if (localValues.ContainsKey("ShowAvatars"))
                ShowAvatars = (bool)localValues["ShowAvatars"];
            else
                ShowAvatars = false;

            if (localValues.ContainsKey("OnlyWIFIDownload"))
                OnlyWIFIDownload = (bool)localValues["OnlyWIFIDownload"];
            else
                OnlyWIFIDownload = true;

            if (localValues.ContainsKey("TransparentTile"))
                TransparentTile = (bool)localValues["TransparentTile"];
            else
                TransparentTile = true;

            if (localValues.ContainsKey("IsBlacklistEnabled"))
                IsBlacklistEnabled = (bool)localValues["IsBlacklistEnabled"];
            else
                IsBlacklistEnabled = true;

            if (localValues.ContainsKey("HotTimeSpan"))
                HotTimeSpan = (int)localValues["HotTimeSpan"];
            else
                HotTimeSpan = 24;

            if (localValues.ContainsKey("ShowPlus18"))
                ShowPlus18 = (bool)localValues["ShowPlus18"];
            else
                ShowPlus18 = false;

            if (localValues.ContainsKey("YTApp"))
                YTApp = IntToYT((int)localValues["YTApp"]);
            else
                YTApp = YoutubeApp.NONE;

            if (localSettings.Containers.ContainsKey("UserInfo"))
            {
                var values = localSettings.Containers["UserInfo"].Values;
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

                App.ApiService.UserInfo = UserInfo;
            }
        }

        public void Save()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var localValues = localSettings.Values;

            localValues["FirstRun"] = FirstRun;
            localValues["NightMode"] = NightMode;
            localValues["FontScaleFactor"] = FontScaleFactor;
            localValues["ShowAvatars"] = ShowAvatars;
            localValues["OnlyWIFIDownload"] = OnlyWIFIDownload;
            localValues["TransparentTile"] = TransparentTile;

            localValues["IsBlacklistEnabled"] = IsBlacklistEnabled;

            localValues["HotTimeSpan"] = HotTimeSpan;
            localValues["ShowPlus18"] = ShowPlus18;

            localValues["YTApp"] = YTtoInt();

            if (UserInfo != null)
            {
                // FIXME
                //UserInfo.HashtagNotificationsCount = App.NotificationsViewModel.HashtagNotificationsCount;
                //UserInfo.AtNotificationsCount = App.NotificationsViewModel.AtNotificationsCount;
                //UserInfo.PMNotificationsCount = App.NotificationsViewModel.PMNotificationsCount;

                var container = localSettings.CreateContainer("UserInfo", Windows.Storage.ApplicationDataCreateDisposition.Always);
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
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var localValues = localSettings.Values;

            localSettings.DeleteContainer("UserInfo");
            localValues.Clear();
            Load();
        }
    }
}
