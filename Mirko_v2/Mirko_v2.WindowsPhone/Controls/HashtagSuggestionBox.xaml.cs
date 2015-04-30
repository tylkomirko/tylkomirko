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
        public delegate void HashtagSelectedEventHandler(object sender, StringEventArgs e);
        public event HashtagSelectedEventHandler HashtagSelected;       

        public HashtagSuggestionBox()
        {
            this.InitializeComponent();

            this.DataContext = this;
        }

        private void GenerateSuggestions(string query)
        {
            var cacheVM = SimpleIoc.Default.GetInstance<CacheViewModel>();
            if (query.Length < 2 || cacheVM.PopularHashtags.Count == 0) return;

            cacheVM.HashtagSuggestions.Clear();
            var sug = cacheVM.PopularHashtags.Where(item => item.StartsWith(query));
            cacheVM.HashtagSuggestions.AddRange(sug);
        }

        private void HashtagBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = this.HashtagBox.Text;
            if (txt == "#") return;

            GenerateSuggestions(txt);
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

            var cacheVM = SimpleIoc.Default.GetInstance<CacheViewModel>();
            cacheVM.HashtagSuggestions.Clear();
        }
    }
}
