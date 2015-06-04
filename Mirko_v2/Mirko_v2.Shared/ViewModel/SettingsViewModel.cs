using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Mirko_v2.Utils;
using NotificationsExtensions.BadgeContent;
using NotificationsExtensions.TileContent;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;

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
            set 
            { 
                RoamingValues["OnlyWIFIDownload"] = value;
                Messenger.Default.Send<NotificationMessage<bool>>(new NotificationMessage<bool>(value, "OnlyWIFI"));
            }
        }

        public bool ShowPlus18
        {
            get { return RoamingValues.ContainsKey("ShowPlus18") ? (bool)RoamingValues["ShowPlus18"] : false; }
            set { RoamingValues["ShowPlus18"] = value; }
        }

        public bool LiveTile
        {
            get { return RoamingValues.ContainsKey("LiveTile") ? (bool)RoamingValues["LiveTile"] : true; }
            set
            {
                if (value)
                    SetLiveTile();
                else
                    ClearLiveTile();

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

        public SettingsViewModel()
        {
            UserInfo = App.ApiService.UserInfo;

            App.ApiService.PropertyChanged += (s, e) => UserInfo = App.ApiService.UserInfo;

            var values = Enum.GetValues(typeof(YouTubeApp));
            _youTubeApps = new List<string>(values.Length);
            foreach (YouTubeApp value in values)
                _youTubeApps.Add(value.GetStringValue());

            Messenger.Default.Register<NotificationMessage>(this, ReadMessage);
            Messenger.Default.Register<NotificationMessage<uint>>(this, ReadMessage);
        }

        private async void ReadMessage(NotificationMessage obj)
        {
            if (obj.Notification == "Init")
            {
                await BackgroundTasksUtils.RegisterTask(typeof(BackgroundTasks.Cleaner).FullName,
                    "Cleaner",
                    new MaintenanceTrigger(60 * 24, false),
                    new SystemCondition(SystemConditionType.UserNotPresent));

                PseudoPushToggled.Execute(null);
            }
        }

        private void ReadMessage(NotificationMessage<uint> obj)
        {
            if (obj.Notification == "Update")
            {
                var count = obj.Content;
                SetBadge(count);
            }
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

        #region Badge/Livetile
        public void SetBadge(uint count)
        {
            BadgeNumericNotificationContent badgeContent = new BadgeNumericNotificationContent(count);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badgeContent.CreateNotification());
        }

        public void ClearBadge()
        {
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
        }

        public void SetLiveTile()
        {
            /* change tiles to badged ones */
            var smallTile = TileContentFactory.CreateTileSquare71x71IconWithBadge();
            smallTile.Branding = TileBranding.None;
            smallTile.ImageIcon.Src = "Assets/small_badge.png";

            var mediumTile = TileContentFactory.CreateTileSquare150x150IconWithBadge();
            mediumTile.Branding = TileBranding.Name;
            mediumTile.ImageIcon.Src = "Assets/medium_badge.png";

            var wideTile = TileContentFactory.CreateTileWide310x150IconWithBadgeAndText();
            wideTile.Branding = TileBranding.Name;
            wideTile.Square150x150Content = mediumTile;
            wideTile.ImageIcon.Src = "Assets/wide_badge.png";

            TileUpdateManager.CreateTileUpdaterForApplication().Update(smallTile.CreateNotification());
            TileUpdateManager.CreateTileUpdaterForApplication().Update(mediumTile.CreateNotification());
            TileUpdateManager.CreateTileUpdaterForApplication().Update(wideTile.CreateNotification());
        }

        public void ClearLiveTile()
        {
            ClearBadge();
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
        }
        #endregion
    }
}
