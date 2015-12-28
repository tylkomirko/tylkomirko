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

    public enum BackgroundImage
    {
        [StringValue("Tapeta")]
        WALLPAPER,
        [StringValue("Ekran blokady")]
        LOCKSCREEN,
        [StringValue("Oba")]
        BOTH
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
        private readonly IPropertySet LocalValues = Windows.Storage.ApplicationData.Current.LocalSettings.Values;
        private IPropertySet SettingsContainer
        {
            get { return SyncSettings ? RoamingValues : LocalValues; }
        }

        public bool PseudoPush
        {
            get { return LocalValues.ContainsKey("PseudoPush") ? (bool)LocalValues["PseudoPush"] : false; }
            set { LocalValues["PseudoPush"] = value; }
        }

        public bool FirstRun
        {
            get { return SettingsContainer.ContainsKey("FirstRun") ? (bool)SettingsContainer["FirstRun"] : true; }
            set { SettingsContainer["FirstRun"] = value; }
        }

        public ElementTheme SelectedTheme
        {
            get { return SettingsContainer.ContainsKey("SelectedTheme") ? (ElementTheme)Enum.Parse(typeof(ElementTheme), (string)SettingsContainer["SelectedTheme"]) : ElementTheme.Dark; }
            set 
            {
                SettingsContainer["SelectedTheme"] = value.ToString(); 
                base.RaisePropertyChanged("SelectedTheme"); 
                if (ThemeChanged != null) 
                    ThemeChanged(this, new ThemeChangedEventArgs(value)); 
            }
        }

        public double FontScaleFactor
        {
            get { return SettingsContainer.ContainsKey("FontScaleFactor") ? (double)SettingsContainer["FontScaleFactor"] : 1.0; }
            set { SettingsContainer["FontScaleFactor"] = value; base.RaisePropertyChanged("FontScaleFactor"); }
        }

        public bool ShowAvatars
        {
            get { return SettingsContainer.ContainsKey("ShowAvatars") ? (bool)SettingsContainer["ShowAvatars"] : false; }
            set { SettingsContainer["ShowAvatars"] = value; base.RaisePropertyChanged("ShowAvatars"); }
        }

        public bool OnlyWIFIDownload
        {
            get { return SettingsContainer.ContainsKey("OnlyWIFIDownload") ? (bool)SettingsContainer["OnlyWIFIDownload"] : false; }
            set 
            {
                SettingsContainer["OnlyWIFIDownload"] = value;
                base.RaisePropertyChanged("OnlyWIFIDownload");
            }
        }

        public bool ShowPlus18
        {
            get { return SettingsContainer.ContainsKey("ShowPlus18") ? (bool)SettingsContainer["ShowPlus18"] : false; }
            set 
            {
                SettingsContainer["ShowPlus18"] = value;
                base.RaisePropertyChanged("ShowPlus18");
            }
        }

        public bool LiveTile
        {
            get { return SettingsContainer.ContainsKey("LiveTile") ? (bool)SettingsContainer["LiveTile"] : true; }
            set
            {
                if (value)
                    NotificationsManager.SetLiveTile();
                else
                    NotificationsManager.ClearLiveTile();

                SettingsContainer["LiveTile"] = value;
            }
        }

        public int HotTimeSpan
        {
            get { return SettingsContainer.ContainsKey("HotTimeSpan") ? (int)SettingsContainer["HotTimeSpan"] : 12; }
            set { SettingsContainer["HotTimeSpan"] = value; }
        }

        private List<string> _youTubeApps;
        public List<string> YouTubeApps
        {
            get { return _youTubeApps; }
        }

        public YouTubeApp SelectedYouTubeApp
        {
            get
            {
                if (SettingsContainer.ContainsKey("YouTubeApp"))
                {
                    YouTubeApp temp = YouTubeApp.IE;
                    Enum.TryParse<YouTubeApp>((string)SettingsContainer["YouTubeApp"], false, out temp);
                    return temp;
                }
                else
                {
                    return YouTubeApp.IE;
                }
            }

            set
            {
                SettingsContainer["YouTubeApp"] = value.ToString();
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
                if (SettingsContainer.ContainsKey("StartPage"))
                {
                    StartPage temp = StartPage.MIRKO;
                    Enum.TryParse<StartPage>((string)SettingsContainer["StartPage"], false, out temp);
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

        public bool SyncSettings
        {
            get { return LocalValues.ContainsKey("SyncSettings") ? (bool)LocalValues["SyncSettings"] : true; }
            set { LocalValues["SyncSettings"] = value; }
        }

#if WINDOWS_UWP
        public bool SetBackground
        {
            get { return LocalValues.ContainsKey("SetBackground") ? (bool)LocalValues["SetBackground"] : false; }
            set { LocalValues["SetBackground"] = value; }
        }

        private RelayCommand<bool> _setBackgroundToggled = null;
        public RelayCommand<bool> SetBackgroundToggled
        {
            get { return _setBackgroundToggled ?? (_setBackgroundToggled = new RelayCommand<bool>(ExecuteSetBackgroundToggled)); }
        }

        private async void ExecuteSetBackgroundToggled(bool setBackground)
        {
            if (setBackground)
            {
                await BackgroundTasksUtils.RegisterTask(typeof(BackgroundTasksUWP.BackgroundImage).FullName,
                                           "BackgroundImage",
                                           new TimeTrigger(60, false),
                                           new SystemCondition(SystemConditionType.InternetAvailable));
            }
            else
            {
                BackgroundTasksUtils.UnregisterTask("BackgroundImage");
            }

            SelectedBackground = SelectedBackground; // dummy write to trigger setting LocalValues in SelectedBackground setter.
            BackgroundHashtag = BackgroundHashtag; // - == -
        }

        private List<string> _backgroundSelections;
        public List<string> BackgroundSelections
        {
            get { return _backgroundSelections; }
        }

        public BackgroundImage SelectedBackground
        {
            get
            {
                if (LocalValues.ContainsKey("BackgroundImageSelection"))
                {
                    BackgroundImage temp = BackgroundImage.WALLPAPER;
                    Enum.TryParse<BackgroundImage>((string)LocalValues["BackgroundImageSelection"], false, out temp);
                    return temp;
                }
                else
                {
                    return BackgroundImage.WALLPAPER;
                }
            }

            set
            {
                LocalValues["BackgroundImageSelection"] = value.ToString();
                
                LocalValues["SetWallpaper"] = false;
                LocalValues["SetLockscreen"] = false;
                if (value == BackgroundImage.WALLPAPER)
                    LocalValues["SetWallpaper"] = true;
                else if (value == BackgroundImage.LOCKSCREEN)
                    LocalValues["SetLockscreen"] = true;
                else
                {
                    LocalValues["SetWallpaper"] = true;
                    LocalValues["SetLockscreen"] = true;
                }
            }
        }

        private RelayCommand<int> _selectedBackgroundChanged = null;
        public RelayCommand<int> SelectedBackgroundChanged
        {
            get { return _selectedBackgroundChanged ?? (_selectedBackgroundChanged = new RelayCommand<int>(ExecuteSelectedBackgroundChanged)); }
        }

        private void ExecuteSelectedBackgroundChanged(int id)
        {
            var values = Enum.GetValues(typeof(BackgroundImage)).Cast<BackgroundImage>();
            SelectedBackground = (BackgroundImage)values.ElementAt(id);
        }

        public string BackgroundHashtag
        {
            get { return LocalValues.ContainsKey("BackgroundHashtag") ? (string)LocalValues["BackgroundHashtag"] : "#earthporn"; }
            set { LocalValues["BackgroundHashtag"] = value; RaisePropertyChanged(); }
        }
#endif

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

#if WINDOWS_UWP
            values = Enum.GetValues(typeof(BackgroundImage));
            _backgroundSelections = new List<string>(values.Length);
            foreach (BackgroundImage value in values)
                _backgroundSelections.Add(value.GetStringValue());
#endif

            Messenger.Default.Register<NotificationMessage>(this, ReadMessage);
        }

        private void ReadMessage(NotificationMessage obj)
        {
            if (obj.Notification == "Init")
            {
                PseudoPushToggled.Execute(PseudoPush);
#if WINDOWS_UWP
                SetBackgroundToggled.Execute(SetBackground);
#endif
            }
        }

        public void Delete()
        {
            Windows.Storage.ApplicationData.Current.RoamingSettings.DeleteContainer("UserInfo");
            RoamingValues.Clear();
            LocalValues.Clear();

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

        private RelayCommand<bool> _pseudoPushToggled = null;
        public RelayCommand<bool> PseudoPushToggled
        {
            get { return _pseudoPushToggled ?? (_pseudoPushToggled = new RelayCommand<bool>(ExecutePseudoPushToggled)); }
        }

        private async void ExecutePseudoPushToggled(bool pseudoPush)
        {
            if (pseudoPush)
            {
                await BackgroundTasksUtils.RegisterTask(typeof(BackgroundTasks.PseudoPush).FullName,
                                        "PseudoPush",
#if WINDOWS_PHONE_APP
                                        new TimeTrigger(30, false),
#else
                                        new TimeTrigger(15, false),
#endif
                                        new SystemCondition(SystemConditionType.InternetAvailable));
            }
            else
            {
                BackgroundTasksUtils.UnregisterTask("PseudoPush");
            }
        }

        private RelayCommand _syncSettingsToggled = null;
        public RelayCommand SyncSettingsToggled
        {
            get { return _syncSettingsToggled ?? (_syncSettingsToggled = new RelayCommand(ExecuteSyncSettingsToggled)); }
        }

        private void ExecuteSyncSettingsToggled()
        {
            IPropertySet input = SyncSettings ? LocalValues : RoamingValues;
            IPropertySet output = SyncSettings ? RoamingValues : LocalValues;

            string[] keys = new string[] { "PseudoPush", "FirstRun", "SelectedTheme", "FontScaleFactor",
                                           "ShowAvatars", "OnlyWIFIDownload", "ShowPlus18", "LiveTile",
                                           "HotTimeSpan", "YouTubeApp", "StartPage",
#if WINDOWS_UWP
                                           "BackgroundImageSelection", "BackgroundHashtag"
#endif
                                           };

            foreach (var key in keys)
                output[key] = input[key];
        }
    }
}
