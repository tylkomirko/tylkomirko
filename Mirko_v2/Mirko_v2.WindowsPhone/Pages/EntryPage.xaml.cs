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
    public sealed partial class EntryPage : UserControl
    {
        public EntryPage()
        {
            this.InitializeComponent();
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var mainVM = this.DataContext as MainViewModel;
            if (mainVM.CommentToScrollInto != null)
                ListView.ScrollIntoView(mainVM.CommentToScrollInto, ScrollIntoViewAlignment.Leading);
        }
    }
}
