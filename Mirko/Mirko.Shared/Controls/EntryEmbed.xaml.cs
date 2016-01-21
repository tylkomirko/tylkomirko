using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Mirko.Utils;
using Mirko.ViewModel;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Controls
{
    public sealed partial class EntryEmbed : UserControl
    {
        private bool singleTap;
        private static SettingsViewModel Settings = null;

        public EntryEmbed()
        {
            this.InitializeComponent();

            if (Settings == null)
                Settings = SimpleIoc.Default.GetInstance<SettingsViewModel>();

            App.ApiService.NetworkStatusChanged += ApiService_NetworkStatusChanged;
            Settings.PropertyChanged += Settings_PropertyChanged;
            this.Unloaded += EntryEmbed_Unloaded;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "OnlyWIFIDownload" || e.PropertyName == "ShowPlus18")
                HandleImageVisibility();
        }

        private void ApiService_NetworkStatusChanged(object sender, WykopSDK.Utils.NetworkEventArgs e)
        {
            HandleImageVisibility();
        }

        private void EntryEmbed_Unloaded(object sender, RoutedEventArgs e)
        {
            Image.Source = null;
        }

        private bool ShowImage(EmbedViewModel VM)
        {
            if (VM.ImageShown || VM.ForceShow)
                return true;

            if (!App.ApiService.IsWIFIAvailable && Settings.OnlyWIFIDownload)
                return false;

            if (!App.ApiService.IsNetworkAvailable)
                return false;

            if (VM.EmbedData.NSFW && !Settings.ShowPlus18)
                return false;

            if (App.ApiService.IsWIFIAvailable)
                return true;

            return true;
        }

        private void HandleImageVisibility()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                var VM = DataContext as EmbedViewModel;
                if (VM == null || VM.EmbedData == null) return;

                if (ShowImage(VM))
                {
                    Image.Source = VM.EmbedData.PreviewURL;
                    Image.Visibility = Visibility.Visible;
                    AttachmentTB.Visibility = Visibility.Collapsed;
                    VM.ImageShown = true;
                }
                else
                {
                    Image.Source = "";
                    Image.Visibility = Visibility.Collapsed;
                    MediaElement.Visibility = Visibility.Collapsed;
                    AttachmentTB.Visibility = Visibility.Visible;
                }
            });
        }

        private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            var VM = DataContext as EmbedViewModel;
            if (VM == null) return;

            if (!VM.ImageShown)
            {
                VM.ForceShow = true;
                VM.ImageShown = true;
            }
            else
            {
                VM.OpenEmbedCommand.Execute(null);
            }
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            Image.Visibility = Visibility.Collapsed;

            var aspectRatio = MediaElement.AspectRatioHeight / (double)MediaElement.AspectRatioWidth;
            MediaElement.Height = MaxHeight * aspectRatio;

            MediaElement.Visibility = Visibility.Visible;

            StatusBarManager.HideProgress();
        }

        private async void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var me = sender as MediaElement;

            var telemetry = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry();
            telemetry.Properties.Add("Message", e.ErrorMessage);
            App.TelemetryClient.TrackException(telemetry);

            try
            {
                var msg = new MessageDialog("Niestety, coś poszło nie tak. Czy chciałbyś otworzyć ten gif w przeglądarce?", "Przykra sprawa");
                msg.Commands.Add(new UICommand("Tak", new UICommandInvokedHandler(async (cmd) =>
                {
                    await Launcher.LaunchUriAsync(me.Source);
                })));
                msg.Commands.Add(new UICommand("Nie", new UICommandInvokedHandler(cmd =>
                {
                    // do... nothing?
                })));

                await msg.ShowAsync();
            }
            catch (Exception ex)
            {
                App.TelemetryClient.TrackException(ex);
            }
            finally
            {
                me.Visibility = Visibility.Collapsed;
            }
        }

        private async void MediaElement_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var me = sender as MediaElement;

            singleTap = true;
            await Task.Delay(100); // ugly trick, I know

            if (singleTap)
            {
                if (me.CurrentState == MediaElementState.Playing)
                    me.Pause();
                else if (me.CurrentState == MediaElementState.Paused)
                    me.Play();
            }
        }

        private void MediaElement_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;
            var me = sender as MediaElement;

            singleTap = false;

            me.IsFullWindow = !me.IsFullWindow;
            Messenger.Default.Send(new NotificationMessage<bool>(me.IsFullWindow, "MediaElement DoubleTapped"));
        }

        private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var embed = args.NewValue as EmbedViewModel;
            if (embed == null || embed.EmbedData == null) return;

            HandleImageVisibility();
            embed.PropertyChanged += (s, e) =>
            {
                if(e.PropertyName == "ForceShow")
                    HandleImageVisibility();
            };
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            var VM = this.DataContext as EmbedViewModel;
            if(VM != null)
                VM.SaveImageCommand.Execute(null);
        }
    }
}
