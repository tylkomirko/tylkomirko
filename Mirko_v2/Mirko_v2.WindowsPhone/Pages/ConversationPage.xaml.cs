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
using WykopAPI.Models;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public class PMTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ReceivedTemplate { get; set; }
        public DataTemplate SentTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var pm = item as PMViewModel;

            if (pm.Data.Direction == MessageDirection.Received)
                return ReceivedTemplate;
            else
                return SentTemplate;
        }
    }

    public sealed partial class ConversationPage : UserControl
    {
        public ConversationPage()
        {
            this.InitializeComponent();

            this.ListView.Loaded += (s, args) =>
            {
                if (this.ListView.ItemsSource != null)
                    this.ListView.ScrollIntoView(this.ListView.Items.Last(), ScrollIntoViewAlignment.Leading);
            };
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //this.BottomAppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            /*
            if (this.TextBox.Text.Length > 0)
                this.BottomAppBar.IsSticky = true;
            else
                this.BottomAppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;*/
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*
            var txt = TextBox.Text;
            if (txt.Length > 0 || AttachmentSymbol.Text.Length > 3) // 3 is the length of attachment symbol and two spaces
                SendButton.IsEnabled = true;
            else
                SendButton.IsEnabled = false;*/
        }

        private void LennyButton_Click(object sender, RoutedEventArgs e)
        {
            var flyout = Resources["LennysFlyout"] as Flyout;
            flyout.ShowAt(this);
        }

        private void LennyChooser_LennySelected(object sender, StringEventArgs e)
        {
            var txt = e.String + " ";
            this.TextBox.Text += txt;
            this.TextBox.SelectionStart = this.TextBox.Text.Length;

            var flyout = this.Resources["LennysFlyout"] as FlyoutBase;
            flyout.Hide();
        }

        private void AttachmentSymbol_Holding(object sender, HoldingRoutedEventArgs e)
        {
            var mf = this.Resources["DeleteAttachmentFlyout"] as MenuFlyout;
            mf.ShowAt(this);
        }
    }
}
