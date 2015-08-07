using MetroLog;
using MetroLog.Targets;
using Newtonsoft.Json;
using NotificationsExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using WykopSDK.API;
using WykopSDK.API.Models;

namespace BackgroundTasks
{
    public sealed class PseudoPush : IBackgroundTask
    {
        private readonly ILogger Logger = null;
        private WykopAPI ApiService;

        public PseudoPush()
        {
            var configuration = new LoggingConfiguration();
#if DEBUG
            configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new DebugTarget());
#endif
            configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new FileStreamingTarget() { RetainDays = 7 });
            configuration.IsEnabled = true;

            try
            {
                LogManagerFactory.DefaultConfiguration = configuration;
            }
            catch (InvalidOperationException) { }

            Logger = LogManagerFactory.DefaultLogManager.GetLogger<PseudoPush>();
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            Logger.Trace("PseudoPush started.");
            
            bool appRunning = false;
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("AppRunning"))
                appRunning = (bool)Windows.Storage.ApplicationData.Current.LocalSettings.Values["AppRunning"];

            if(appRunning)
                Logger.Trace("App is running.");

            ApiService = new WykopAPI();

            if (ApiService.UserInfo == null ||
                appRunning ||
                string.IsNullOrEmpty(ApiService.UserInfo.UserName) ||
                string.IsNullOrEmpty(ApiService.UserInfo.AccountKey))
            {
                Logger.Trace("Terminating.");

                deferral.Complete();
                return;
            }

            // everything is fine, let's log in and check notifications
            var hashtagCount = await ApiService.getHashtagNotificationsCount();
            var notifications = await GetNotifications();

            Logger.Trace(hashtagCount.Count + " hashtag notifications");
            Logger.Trace(notifications.Count + " new notifications");

            if (notifications.Count > 0)
            {
                SendToasts(notifications);
                ApiService.UserInfo.LastToastDate = notifications.First().Date;
            }

            NotificationsManager.SetBadge((uint)(hashtagCount.Count + notifications.Count));
            ApiService.SaveUserInfo();
            ApiService.Dispose();

            Windows.Storage.ApplicationData.Current.LocalSettings.Values["PseudoPushLastTime"] = DateTime.Now.ToBinary();

            deferral.Complete();
        }

        private async Task<List<Notification>> GetNotifications()
        {
            var lastToast = ApiService.UserInfo.LastToastDate;
            List<Notification> temp = null;
            List<Notification> notifications = new List<Notification>(1);
            uint pageIndex = 0;

            var supportedTypes = new NotificationType[] 
            { 
                NotificationType.Observe, NotificationType.Unobserve, 
                NotificationType.EntryDirected, NotificationType.CommentDirected,
                NotificationType.System, NotificationType.Badge, NotificationType.PM,
            };

            do
            {
                temp = await ApiService.getNotifications(pageIndex++);
                if (temp == null) 
                    break;
                
                var newNotifications = temp.Where(x => x.IsNew).Where(x => supportedTypes.Contains(x.Type)).Where(x => x.Date > lastToast);
                //var newNotifications = temp.Where(x => x.Type == NotificationType.EntryDirected).Take(1);
                if (newNotifications.Count() == 0)
                    break;
                else
                    notifications.AddRange(newNotifications);

                if (!temp.Last().IsNew)
                    break;

            } while (true);

            return notifications;
        }

        #region LIVETILE
        /*
        private async Task CreateMediumCycleTile1()
        {
            var tile = new Canvas()
            {
                Height = 210,
                Width = 210,
                Background = new SolidColorBrush(Colors.Transparent),
            };

            // add logo
            Image logo = new Image();
            var img = new BitmapImage() { CreateOptions = BitmapCreateOptions.None };
            img.UriSource = new Uri("ms-appx:///Assets/CycleIconMedium1.png");
            logo.Source = img;
            tile.Children.Add(logo);

            var textBlock = new TextBlock()
            {
                Text = (notificationsCount[0] + notificationsCount[1] + notificationsCount[2]).ToString(),
                FontSize = 100,
            };

            Canvas.SetTop(textBlock, 110.0);
            Canvas.SetLeft(textBlock, 205.0);
            tile.Children.Add(textBlock);

            RenderTargetBitmap rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(tile);
            IBuffer pixels = await rtb.GetPixelsAsync();
            using (DataReader dReader = Windows.Storage.Streams.DataReader.FromBuffer(pixels))
            {
                byte[] data = new byte[pixels.Length];
                dReader.ReadBytes(data);

                var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                var outputFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.CreateFileAsync("CycleTile1.png", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                using (var outputStream = await outputFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                {
                    BitmapEncoder enc = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, outputStream);
                    enc.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)rtb.PixelWidth, (uint)rtb.PixelHeight, logicalDpi, logicalDpi, data);
                    await enc.FlushAsync();
                }
            }
        }

        private async Task CreateMediumCycleTile2()
        {
            var tile = new Canvas()
            {
                Height = 210,
                Width = 210,
                Background = new SolidColorBrush(Colors.Transparent),
            };

            // add logo
            Image logo = new Image();
            var img = new BitmapImage() { CreateOptions = BitmapCreateOptions.None };
            img.UriSource = new Uri("ms-appx:///Assets/CycleIconMedium2.png");
            logo.Source = img;
            tile.Children.Add(logo);

            var fontSize = 40;
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto } );
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            var hashtag = new TextBlock()
            {
                FontSize = fontSize,
                Text = "#",
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var hashtagCount = new TextBlock()
            {
                FontSize = fontSize,
                Margin = new Thickness(15.0, 0, 0, 0),
                Text = notificationsCount[0].ToString(),
            };

            Grid.SetColumn(hashtag, 0);
            Grid.SetRow(hashtag, 0);
            Grid.SetColumn(hashtagCount, 1);
            Grid.SetRow(hashtagCount, 0);
            grid.Children.Add(hashtag);
            grid.Children.Add(hashtagCount);

            var at = new TextBlock()
            {
                FontSize = fontSize,
                Text = "@",
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var atCount = new TextBlock()
            {
                FontSize = fontSize,
                Margin = new Thickness(15.0, 0, 0, 0),
                Text = notificationsCount[1].ToString(),
            };

            Grid.SetColumn(at, 0);
            Grid.SetRow(at, 1);
            Grid.SetColumn(atCount, 1);
            Grid.SetRow(atCount, 1);
            grid.Children.Add(at);
            grid.Children.Add(atCount);


            var pm = new TextBlock()
            {
                FontSize = fontSize,
                Text = "\u2709",
                IsColorFontEnabled = false,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var pmCount = new TextBlock()
            {
                FontSize = fontSize,
                Margin = new Thickness(15.0, 0, 0, 0),
                Text = notificationsCount[2].ToString(),
            };

            Grid.SetColumn(pm, 0);
            Grid.SetRow(pm, 2);
            Grid.SetColumn(pmCount, 1);
            Grid.SetRow(pmCount, 2);
            grid.Children.Add(pm);
            grid.Children.Add(pmCount);

            Canvas.SetTop(grid, 18.0);
            Canvas.SetLeft(grid, 14.0);
            tile.Children.Add(grid);

            RenderTargetBitmap rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(tile);
            IBuffer pixels = await rtb.GetPixelsAsync();
            using (DataReader dReader = Windows.Storage.Streams.DataReader.FromBuffer(pixels))
            {
                byte[] data = new byte[pixels.Length];
                dReader.ReadBytes(data);

                var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                var outputFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.CreateFileAsync("CycleTile2.png", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                using (var outputStream = await outputFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                {
                    BitmapEncoder enc = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, outputStream);
                    enc.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)rtb.PixelWidth, (uint)rtb.PixelHeight, logicalDpi, logicalDpi, data);
                    await enc.FlushAsync();
                }
            }
        }

        private async Task CreateLargeCycleTile1()
        {
            var tile = new Canvas()
            {
                Height = 210,
                Width = 432,
                Background = new SolidColorBrush(Colors.Transparent),
            };

            // add logo
            Image logo = new Image();
            var img = new BitmapImage() { CreateOptions = BitmapCreateOptions.None };
            img.UriSource = new Uri("ms-appx:///Assets/CycleIconLarge1.png");
            logo.Source = img;
            tile.Children.Add(logo);

            var textBlock = new TextBlock()
            {
                Text = (notificationsCount[0] + notificationsCount[1] + notificationsCount[2]).ToString(),
                FontSize = 110,
            };

            Canvas.SetTop(textBlock, (tile.ActualHeight - textBlock.ActualHeight)/2);
            Canvas.SetLeft(textBlock, 500.0);
            tile.Children.Add(textBlock);

            RenderTargetBitmap rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(tile);
            IBuffer pixels = await rtb.GetPixelsAsync();
            using (DataReader dReader = Windows.Storage.Streams.DataReader.FromBuffer(pixels))
            {
                byte[] data = new byte[pixels.Length];
                dReader.ReadBytes(data);

                var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                var outputFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.CreateFileAsync("LargeCycleTile1.png", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                using (var outputStream = await outputFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                {
                    BitmapEncoder enc = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, outputStream);
                    enc.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)rtb.PixelWidth, (uint)rtb.PixelHeight, logicalDpi, logicalDpi, data);
                    await enc.FlushAsync();
                }
            }
        }

        private async Task CreateLargeCycleTile2()
        {
            var tile = new Canvas()
            {
                Height = 336,
                Width = 691,
                Background = new SolidColorBrush(Colors.Transparent),
            };

            // add logo
            var fontSize = 50;
            Image logo = new Image();
            var img = new BitmapImage() { CreateOptions = BitmapCreateOptions.None };
            img.UriSource = new Uri("ms-appx:///Assets/CycleIconLarge2.png");
            logo.Source = img;
            tile.Children.Add(logo);

            var panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
            };

            var tb1 = new TextBlock()
            {
                FontSize = fontSize,
                Text = "# " + notificationsCount[0],
            };

            var tb2 = new TextBlock()
            {
                FontSize = fontSize,
                Margin = new Thickness(55, 0, 0, 0),
                Text = "@ " + notificationsCount[1],
            };

            var tb3 = new TextBlock()
            {
                FontSize = fontSize,
                Margin = new Thickness(55, 0, 0, 0),
                IsColorFontEnabled = false,
                Text = "\u2709 " + notificationsCount[2],
            };

            panel.Children.Add(tb1);
            panel.Children.Add(tb2);
            panel.Children.Add(tb3);
            Canvas.SetLeft(panel, 20);
            Canvas.SetTop(panel, 20);
            tile.Children.Add(panel);

            var tb = new TextBlock()
            {
                FontSize= fontSize,
                Text = "",//notificationText,
                TextWrapping = TextWrapping.Wrap,
            };

            Canvas.SetLeft(tb, 20);
            Canvas.SetTop(tb, (tile.ActualHeight - tb.ActualHeight)/2);
            tile.Children.Add(tb);

            RenderTargetBitmap rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(tile);
            IBuffer pixels = await rtb.GetPixelsAsync();
            using (DataReader dReader = Windows.Storage.Streams.DataReader.FromBuffer(pixels))
            {
                byte[] data = new byte[pixels.Length];
                dReader.ReadBytes(data);

                var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                var outputFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.CreateFileAsync("LargeCycleTile2.png", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                using (var outputStream = await outputFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                {
                    BitmapEncoder enc = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, outputStream);
                    enc.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)rtb.PixelWidth, (uint)rtb.PixelHeight, logicalDpi, logicalDpi, data);
                    await enc.FlushAsync();
                }
            }
        }

        private void UpdateTiles()
        {
            var mgr = TileUpdateManager.CreateTileUpdaterForApplication();

            mgr.Clear();
            mgr.EnableNotificationQueue(true);

            // small tile
            var tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare71x71IconWithBadge);
            var tileImage = tileXml.GetElementsByTagName("image")[0] as XmlElement;
            tileImage.SetAttribute("src", "ms-appx:///Assets/mirko_icon_33x33.scale-140.png");
            var tileNotification = new TileNotification(tileXml);
            mgr.Update(tileNotification);

            // medium tile
            tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150IconWithBadge);
            tileImage = tileXml.GetElementsByTagName("image")[0] as XmlElement;
            tileImage.SetAttribute("src", "ms-appx:///Assets/mirko_icon_62x62.scale-140.png");
            var tileBranding = tileXml.GetElementsByTagName("binding")[0] as XmlElement;
            //tileBranding.SetAttribute("branding", "none");
            tileNotification = new TileNotification(tileXml);
            mgr.Update(tileNotification);

            // large tile
            tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150IconWithBadgeAndText);
            tileImage = tileXml.GetElementsByTagName("image")[0] as XmlElement;
            tileImage.SetAttribute("src", "ms-appx:///Assets/mirko_icon_62x62.scale-140.png");
            tileNotification = new TileNotification(tileXml);
            mgr.Update(tileNotification);
        }
         * */
        #endregion

        #region NOTIFICATIONS
        private void SendToasts(List<Notification> list)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();

            foreach (var notification in list)
            {
                ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
                XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
                IXmlNode toastNode = toastXml.SelectSingleNode("/toast");

                ((XmlElement)toastNode).SetAttribute("launch", JsonConvert.SerializeObject(notification, Newtonsoft.Json.Formatting.None));

                XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
                if (notification.Type == NotificationType.EntryDirected)
                {
                    string headline = "@" + notification.AuthorName + " woła Cię";
                    string body = notification.Entry.Text;

                    toastTextElements[0].AppendChild(toastXml.CreateTextNode(headline));
                    toastTextElements[1].AppendChild(toastXml.CreateTextNode(body));
                }
                else if(notification.Type == NotificationType.CommentDirected)
                {
                    string headline = "@" + notification.AuthorName;
                    headline += (notification.AuthorSex == UserSex.Female) ? " napisała" : " napisał";
                    headline += " do Ciebie w komentarzu.";

                    string body = notification.Entry.Text;

                    toastTextElements[0].AppendChild(toastXml.CreateTextNode(headline));
                    toastTextElements[1].AppendChild(toastXml.CreateTextNode(body));
                }
                else if (notification.Type == NotificationType.PM)
                {
                    string headline = "Nowa prywatna wiadomość";
                    string body = "od @" + notification.AuthorName;

                    toastTextElements[0].AppendChild(toastXml.CreateTextNode(headline));
                    toastTextElements[1].AppendChild(toastXml.CreateTextNode(body));
                }
                else
                {
                    var notificationText = notification.Text.Replace(@"""", "").Replace("  ", " ");
                    toastTextElements[1].AppendChild(toastXml.CreateTextNode(notificationText));
                }

                ToastNotification toast = new ToastNotification(toastXml);
                notifier.Show(toast);
            }
        }
        #endregion
    }
}
