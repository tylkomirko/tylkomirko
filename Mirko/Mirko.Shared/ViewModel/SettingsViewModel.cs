using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Mirko.Utils;
using NotificationsExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using WykopSDK.API;

namespace Mirko.ViewModel
{
    public class ThemeChangedEventArgs : EventArgs
    {
        public ElementTheme Theme { get; set; }

        public ThemeChangedEventArgs(ElementTheme t)
        {
            Theme = t;
        }
    };

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

    public enum StartPage
    {
        [StringValue("Mirko")]
        MIRKO,
        [StringValue("Gorące")]
        HOT,
        [StringValue("Ulubione")]
        FAV,
        [StringValue("Mój Wykop")]
        MY,
    };

    public class SettingsViewModel : ViewModelBase
    {
        private UserInfo _userInfo = null;
        public UserInfo UserInfo
        {
            get { return _userInfo; }
            set { Set(() => UserInfo, ref _userInfo, value); }
        }

        public delegate void ThemeChangedEventHandler(object sender, ThemeChangedEventArgs e);
        public event ThemeChangedEventHandler ThemeChanged;     

        private readonly IPropertySet RoamingValues = Windows.Storage.ApplicationData.Current.RoamingSettings.Values;

        public bool PseudoPush
        {
            get { return RoamingValues.ContainsKey("PseudoPush") ? (bool)RoamingValues["PseudoPush"] : false; }
            set { RoamingValues["PseudoPush"] = value; }
        }

        public bool FirstRun
        {
            get { return RoamingValues.ContainsKey("FirstRun") ? (bool)RoamingValues["FirstRun"] : true; }
            set { RoamingValues["FirstRun"] = value; }
        }

        public ElementTheme SelectedTheme
        {
            get { return RoamingValues.ContainsKey("SelectedTheme") ? (ElementTheme)Enum.Parse(typeof(ElementTheme), (string)RoamingValues["SelectedTheme"]) : ElementTheme.Dark; }
            set 
            { 
                RoamingValues["SelectedTheme"] = value.ToString(); 
                base.RaisePropertyChanged("SelectedTheme"); 
                if (ThemeChanged != null) 
                    ThemeChanged(this, new ThemeChangedEventArgs(value)); 
            }
        }

        public double FontScaleFactor
        {
            get { return RoamingValues.ContainsKey("FontScaleFactor") ? (double)RoamingValues["FontScaleFactor"] : 1.0; }
            set { RoamingValues["FontScaleFactor"] = value; base.RaisePropertyChanged("FontScaleFactor"); }
        }

        public bool ShowAvatars
        {
            get { return RoamingValues.ContainsKey("ShowAvatars") ? (bool)RoamingValues["ShowAvatars"] : false; }
            set { RoamingValues["ShowAvatars"] = value; }
        }

        public bool OnlyWIFIDownload
        {
            get { return RoamingValues.ContainsKey("OnlyWIFIDownload") ? (bool)RoamingValues["OnlyWIFIDownload"] : false; }
            set 
            { 
                RoamingValues["OnlyWIFIDownload"] = value;
                base.RaisePropertyChanged("OnlyWIFIDownload");
            }
        }

        public bool ShowPlus18
        {
            get { return RoamingValues.ContainsKey("ShowPlus18") ? (bool)RoamingValues["ShowPlus18"] : false; }
            set 
            { 
                RoamingValues["ShowPlus18"] = value;
                base.RaisePropertyChanged("ShowPlus18");
            }
        }

        public bool LiveTile
        {
            get { return RoamingValues.ContainsKey("LiveTile") ? (bool)RoamingValues["LiveTile"] : true; }
            set
            {
                if (value)
                    NotificationsManager.SetLiveTile();
                else
                    NotificationsManager.ClearLiveTile();

                RoamingValues["LiveTile"] = value;
            }
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
                if (RoamingValues.ContainsKey("YouTubeApp"))
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

        private List<string> _startPages;
        public List<string> StartPages
        {
            get { return _startPages; }
        }

        public StartPage SelectedStartPage
        {
            get
            {
                if (RoamingValues.ContainsKey("StartPage"))
                {
                    StartPage temp = StartPage.MIRKO;
                    Enum.TryParse<StartPage>((string)RoamingValues["StartPage"], false, out temp);
                    return temp;
                }
                else
                {
                    return StartPage.MIRKO;
                }
            }

            set
            {
                RoamingValues["StartPage"] = value.ToString();
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

            values = Enum.GetValues(typeof(StartPage));
            _startPages = new List<string>(values.Length);
            foreach (StartPage value in values)
                _startPages.Add(value.GetStringValue());

            Messenger.Default.Register<NotificationMessage>(this, ReadMessage);
        }

        private async void ReadMessage(NotificationMessage obj)
        {
            if (obj.Notification == "Init")
            {
                await BackgroundTasksUtils.RegisterTask(typeof(BackgroundTasks.Cleaner).FullName,
                    "Cleaner",
                    new MaintenanceTrigger(60 * 6, false),
                    new SystemCondition(SystemConditionType.UserNotPresent));

                PseudoPushToggled.Execute(null);
            }
        }

        public void Delete()
        {
            Windows.Storage.ApplicationData.Current.RoamingSettings.DeleteContainer("UserInfo");
            RoamingValues.Clear();

            App.ApiService.UserInfo = null;
            NotificationsManager.ClearLiveTile();
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

        private RelayCommand<int> _startPageChanged = null;
        public RelayCommand<int> StartPageChanged
        {
            get { return _startPageChanged ?? (_startPageChanged = new RelayCommand<int>(ExecuteStartPageChanged)); }
        }

        private void ExecuteStartPageChanged(int id)
        {
            var values = Enum.GetValues(typeof(StartPage)).Cast<StartPage>();
            SelectedStartPage = (StartPage)values.ElementAt(id);
        }

        private RelayCommand _pseudoPushToggled = null;
        public RelayCommand PseudoPushToggled
        {
            get { return _pseudoPushToggled ?? (_pseudoPushToggled = new RelayCommand(ExecutePseudoPushToggled)); }
        }

        private async void ExecutePseudoPushToggled()
        {
            if (PseudoPush)
            {
                await BackgroundTasksUtils.RegisterTask(typeof(BackgroundTasks.PseudoPush).FullName,
                                        "PseudoPush",
                                        new TimeTrigger(30, false),
                                        new SystemCondition(SystemConditionType.InternetAvailable));
            }
            else
            {
                BackgroundTasksUtils.UnregisterTask("PseudoPush");
            }
        }
    }
}
