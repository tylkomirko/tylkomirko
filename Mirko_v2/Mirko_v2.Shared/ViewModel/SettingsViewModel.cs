using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Mirko_v2.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Mirko_v2.ViewModel
{
    public enum YouTubeApp
    {
        [StringValue("Internet Explorer")]
        IE,
        [StringValue("TubeCast")]
        TUBECAST,
        [StringValue("MetroTube")]
        METROTUBE,
        [StringValue("toib")]
        TOIB,
        [StringValue("MyTube")]
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

        private List<string> _youTubeApps;
        public List<string> YouTubeApps
        {
            get { return _youTubeApps; }
        }

        private YouTubeApp _selectedYouTubeApp = YouTubeApp.IE;
        public YouTubeApp SelectedYouTubeApp
        {
            get { return _selectedYouTubeApp; }
            set { Set(() => SelectedYouTubeApp, ref _selectedYouTubeApp, value);}
        }

        public SettingsViewModel()
        {
            Windows.Storage.ApplicationData.Current.DataChanged += (s, o) => Load();

            var values = Enum.GetValues(typeof(YouTubeApp));
            _youTubeApps = new List<string>(values.Length);
            foreach (YouTubeApp value in values)
                _youTubeApps.Add(value.GetStringValue());

            Load();
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

            if (roamingValues.ContainsKey("YouTubeApp"))
                Enum.TryParse<YouTubeApp>((string)roamingValues["YouTubeApp"], false, out _selectedYouTubeApp);
            else
                SelectedYouTubeApp = YouTubeApp.IE;

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

            roamingValues["YouTubeApp"] = SelectedYouTubeApp.ToString();

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

        private RelayCommand _saveCommand = null;
        public RelayCommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(() => Save())); }
        }

        private RelayCommand<int> _youTubeAppChanged = null;
        public RelayCommand<int> YouTubeAppChanged
        {
            get { return _youTubeAppChanged ?? (_youTubeAppChanged = new RelayCommand<int>(ExecuteYouTubeAppChanged)); }
        }

        private void ExecuteYouTubeAppChanged(int id)
        {
            var values = Enum.GetValues(typeof(YouTubeApp)).Cast<YouTubeApp>();
            SelectedYouTubeApp = (YouTubeApp)values.ElementAt(id);
            Save();
        }

    }
}
