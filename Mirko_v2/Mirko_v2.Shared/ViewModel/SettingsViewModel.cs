using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Mirko_v2.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Windows.Storage;
using Windows.Foundation.Collections;

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

        private readonly IPropertySet RoamingValues = Windows.Storage.ApplicationData.Current.RoamingSettings.Values;

        public bool FirstRun
        {
            get { return RoamingValues.ContainsKey("FirstRun") ? (bool)RoamingValues["FirstRun"] : true; }
            set { RoamingValues["FirstRun"] = value; }
        }

        public bool NightMode 
        {
            get { return RoamingValues.ContainsKey("NightMode") ? (bool)RoamingValues["NightMode"] : true; }
            set { RoamingValues["NightMode"] = value; }
        }

        public double FontScaleFactor
        {
            get { return RoamingValues.ContainsKey("FontScaleFactor") ? (double)RoamingValues["FontScaleFactor"] : 1.0; }
            set { RoamingValues["FontScaleFactor"] = value; }
        }

        public bool ShowAvatars
        {
            get { return RoamingValues.ContainsKey("ShowAvatars") ? (bool)RoamingValues["ShowAvatars"] : false; }
            set { RoamingValues["ShowAvatars"] = value; }
        }

        public bool OnlyWIFIDownload
        {
            get { return RoamingValues.ContainsKey("OnlyWIFIDownload") ? (bool)RoamingValues["OnlyWIFIDownload"] : false; }
            set { RoamingValues["OnlyWIFIDownload"] = value; }
        }

        public bool ShowPlus18
        {
            get { return RoamingValues.ContainsKey("ShowPlus18") ? (bool)RoamingValues["ShowPlus18"] : false; }
            set { RoamingValues["ShowPlus18"] = value; }
        }

        public bool LiveTile
        {
            get { return RoamingValues.ContainsKey("LiveTile") ? (bool)RoamingValues["LiveTile"] : true; }
            set { RoamingValues["LiveTile"] = value; }
        }

        public int HotTimeSpan
        {
            get { return RoamingValues.ContainsKey("HotTimeSpan") ? (int)RoamingValues["HotTimeSpan"] : 12; }
            set { RoamingValues["HotTimeSpan"] = value; }
        }

        //public bool IsBlacklistEnabled { get; set; }

        private List<string> _youTubeApps;
        public List<string> YouTubeApps
        {
            get { return _youTubeApps; }
        }

        public YouTubeApp SelectedYouTubeApp
        {
            get
            {
                if(RoamingValues.ContainsKey("YouTubeApp"))
                {
                    YouTubeApp temp = YouTubeApp.IE;
                    Enum.TryParse<YouTubeApp>((string)RoamingValues["YouTubeApp"], false, out temp);
                    return temp;
                }
                else
                {
                    return YouTubeApp.IE;
                }
            }

            set
            {
                RoamingValues["YouTubeApp"] = value.ToString();
            }
        }

        public SettingsViewModel()
        {
            UserInfo = App.ApiService.UserInfo;

            App.ApiService.PropertyChanged += (s, e) => UserInfo = App.ApiService.UserInfo;

            var values = Enum.GetValues(typeof(YouTubeApp));
            _youTubeApps = new List<string>(values.Length);
            foreach (YouTubeApp value in values)
                _youTubeApps.Add(value.GetStringValue());
        }

        public void Delete()
        {
            Windows.Storage.ApplicationData.Current.RoamingSettings.DeleteContainer("UserInfo");
            RoamingValues.Clear();

            App.ApiService.UserInfo = null;
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
        }

    }
}
