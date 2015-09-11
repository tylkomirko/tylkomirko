using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Mirko.Utils;
using Mirko.ViewModel;
using System;
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

            App.ApiService.NetworkStatusChanged += (s, e) => HandleImageVisibility();
            Settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "OnlyWIFIDownload" || e.PropertyName == "ShowPlus18")
                    HandleImageVisibility();
            };
        }

        private void HandleImageVisibility()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                var VM = DataContext as EmbedViewModel;
                if (VM == null || VM.EmbedData == null) return;

                if ((!App.ApiService.IsNetworkAvailable && !VM.ImageShown) ||
                    (Settings.OnlyWIFIDownload && !App.ApiService.IsWIFIAvailable && !VM.ImageShown) ||
                    (VM.EmbedData.NSFW && !Settings.ShowPlus18))
                {
                    Image.Visibility = Visibility.Collapsed;
                    AttachmentTB.Visibility = Visibility.Visible;

                    VM.ImageShown = false;
                }
                else
                {
                    Image.Visibility = Visibility.Visible;
                    AttachmentTB.Visibility = Visibility.Collapsed;

                    VM.ImageShown = true;
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
                Image.Visibility = Visibility.Visible;
                AttachmentTB.Visibility = Visibility.Collapsed;

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
            if (embed == null || embed.EmbedData == null || embed.ImageShown) return;

            HandleImageVisibility();
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as EmbedViewModel;
            vm.SaveImageCommand.Execute(null);
        }

        private async void RefreshImage_Click(object sender, RoutedEventArgs e)
        {
            await Image.RefreshImage();
        }
    }
}
