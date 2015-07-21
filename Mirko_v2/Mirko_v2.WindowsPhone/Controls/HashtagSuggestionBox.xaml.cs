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

namespace Mirko_v2.Controls
{
    public sealed partial class HashtagSuggestionBox : UserControl
    {
        private static CacheViewModel Cache = null;

        public delegate void HashtagSelectedEventHandler(object sender, StringEventArgs e);
        public event HashtagSelectedEventHandler HashtagSelected;       

        public HashtagSuggestionBox()
        {
            this.InitializeComponent();

            if (Cache == null)
            {
                Cache = SimpleIoc.Default.GetInstance<CacheViewModel>();
                if (Cache.PopularHashtags.Count == 0)
                    Cache.GetPopularHashtags();
            }
        }

        private void HashtagBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = HashtagBox.Text;
            if (query.StartsWith("#"))
                Cache.GenerateSuggestions(query);
        }

        private void HashtagBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var tag = this.HashtagBox.Text;

            if (e.Key == Windows.System.VirtualKey.Enter && tag.Length > 2)
            {
                if(this.HashtagSelected != null)
                    this.HashtagSelected(this, new StringEventArgs(tag));

                this.HashtagBox.Text = "#";
                this.HashtagBox.SelectionStart = 1;
            }
        }

        private void SuggestionsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var tag = e.ClickedItem as string;

            if(this.HashtagSelected != null)
                this.HashtagSelected(this, new StringEventArgs(tag));

            this.HashtagBox.Text = "#";
            this.HashtagBox.SelectionStart = 1;

            Cache.HashtagSuggestions.Clear();
        }
    }
}
