using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.Utils;
using Mirko_v2.ViewModel;
using System;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Controls
{
    public sealed partial class EntryEmbed : UserControl
    {
        private static SettingsViewModel Settings = null;

        public EntryEmbed()
        {
            this.InitializeComponent();

            if (Settings == null)
                Settings = SimpleIoc.Default.GetInstance<SettingsViewModel>();
        }

        private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            if(DataContext != null)
            {
                if (Image.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
                {
                    Image.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    AttachmentTB.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
                else
                {
                    (DataContext as EmbedViewModel).OpenEmbedCommand.Execute(null);
                }
            }
        }

        private async void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            Image.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            await StatusBarManager.HideProgress();
        }

        private async void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var me = sender as MediaElement;

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

            me.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void MediaElement_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            var me = sender as MediaElement;

            if (me.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing)
                me.Pause();
            else if (me.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Paused)
                me.Play();
        }

        private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue == null) return;

            var embed = args.NewValue as EmbedViewModel;
            var data = embed.EmbedData;

            if (data == null) return;

            if(data.NSFW && !Settings.ShowPlus18)
            {
                Image.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                AttachmentTB.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }
    }
}
