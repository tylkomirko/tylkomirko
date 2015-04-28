using GalaSoft.MvvmLight.Messaging;
using Mirko_v2.Utils;
using Mirko_v2.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WykopAPI.Models;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Controls
{
    public sealed partial class EntryEmbed : UserControl
    {
        public delegate void PageNavigationEventHandler(object sender, PageNavigationEventArgs e);
        public event PageNavigationEventHandler NavigateTo;

        public EntryEmbed()
        {
            this.InitializeComponent();

            Messenger.Default.Register<string>(this, "Embed opened.", (url) =>
            {
                if (DataContext != null)
                {
                    var vm = DataContext as EmbedViewModel;
                    if (vm.EmbedData != null && vm.EmbedData.URL == url)
                    {
                        //Image.Visibility = Visibility.Collapsed;
                        //MediaElement.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                }
            });
        }

        private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            if(DataContext != null)
            {
                (DataContext as EmbedViewModel).OpenEmbedCommand.Execute(null);
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
    }
}
