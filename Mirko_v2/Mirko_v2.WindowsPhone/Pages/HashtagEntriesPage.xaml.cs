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
    public sealed partial class HashtagEntriesPage : UserControl
    {
        public HashtagEntriesPage()
        {
            this.InitializeComponent();
        }

        private void NewEntriesPopup_Tapped(object sender, TappedRoutedEventArgs e)
        {
            /*
            App.MainViewModel.TaggedEntries.AddNewEntries();

            HideNewEntriesPopup();

            var sv = ListView.GetDescendant<ScrollViewer>();
            sv.ChangeView(null, 0.0, null);*/
        }

        private void ListView_ScrollingDown(object sender, EventArgs e)
        {
            HideHeader.Begin();
        }

        private void ListView_ScrollingUp(object sender, EventArgs e)
        {
            ShowHeader.Begin();
        }
    }


}
