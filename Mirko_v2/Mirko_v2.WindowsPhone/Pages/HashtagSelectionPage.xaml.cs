using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.Utils;
using Mirko_v2.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public class HashtagSelectionPageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; }
        public DataTemplate ItemTemplateNoCount { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var VM = SimpleIoc.Default.GetInstance<NotificationsViewModel>();

            if (VM.HashtagsDictionary.Count > 0)
                return ItemTemplate;
            else
                return ItemTemplateNoCount;
        }
    }

    public sealed partial class HashtagSelectionPage : UserControl
    {
        public HashtagSelectionPage()
        {
            this.InitializeComponent();
        }

        private void HashtagSuggestionBox_HashtagSelected(object sender, StringEventArgs e)
        {
            var tag = e.String;
            var flyout = Resources["HashtagFlyout"] as FlyoutBase;
            flyout.Hide();
            //this.Frame.Navigate(typeof(HashtagEntriesPage), tag);
            // FIXME
        }

        private void FindHashtag_Click(object sender, RoutedEventArgs e)
        {
            var flyout = Resources["HashtagFlyout"] as FlyoutBase;
            flyout.ShowAt(this);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var newItem = e.ClickedItem as HashtagInfoContainer;
            if (newItem == null) return;

            var VM = SimpleIoc.Default.GetInstance<NotificationsViewModel>();
            VM.CurrentHashtag = newItem;
            VM.GoToHashtagNotificationsPage.Execute(null);
        }
    }
}
