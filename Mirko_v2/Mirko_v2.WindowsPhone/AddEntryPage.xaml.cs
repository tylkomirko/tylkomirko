using Mirko_v2.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Mirko_v2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddEntryPage : Page
    {
        public AddEntryPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void AppendTextAndHideFlyout(string text, string flyoutName)
        {
            var start = this.Editor.SelectionStart;
            var newText = this.Editor.Text.Insert(start, text);

            this.Editor.Text = newText;
            this.Editor.SelectionStart = start + newText.Length;

            var f = this.Resources[flyoutName] as FlyoutBase;
            f.Hide();
        }


        private void LennyGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var text = (string)e.ClickedItem + " ";

            AppendTextAndHideFlyout(text, "LennysFlyout");
        }

        private void HyperlinkFlyoutButton_Click(object sender, RoutedEventArgs e)
        {
            string txt = "[" + this.DescriptionTextBox.Text + "](" + this.LinkTextBox.Text + ")";
            AppendTextAndHideFlyout(txt, "HyperlinkFlyout");
        }

        private void LennyChooser_LennySelected(object sender, StringEventArgs e)
        {
            var txt = e.String + " ";
            AppendTextAndHideFlyout(txt, "LennysFlyout");
        }

        private void FormattingHelper(string tag)
        {
            string text = string.Empty;
            string iconPath = string.Empty;
            string label = string.Empty;
            switch (tag)
            {
                case "bold":
                    text = @"**"; iconPath = "appbar.text.bold.png"; label = "pogrubienie"; break;
                case "italic":
                    text = @"_"; iconPath = "appbar.text.italic.png"; label = "pochylenie"; break;
                case "code":
                    text = @"`"; iconPath = "appbar.grade.c.png"; label = "kod"; break;
            }

            this.Editor.Text += text;
            this.Editor.SelectionStart = this.Editor.Text.Length;

            var icon = new BitmapIcon();
            icon.UriSource = new Uri("ms-appx:///Assets/" + iconPath);
            this.FormattingHelperButton.Icon = icon;
            this.FormattingHelperButton.Label = label;
            this.FormattingHelperButton.IsChecked = true;
            this.FormattingHelperButton.Tag = tag;

            this.FormattingHelperButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.SpoilerButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            this.Editor.Focus(FocusState.Programmatic);
        }

        private void FormattingHelperButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var tag = (sender as AppBarToggleButton).Tag.ToString();
            string text = string.Empty;
            switch (tag)
            {
                case "bold":
                    text = @"** "; break;
                case "italic":
                    text = @"_ "; break;
                case "code":
                    text = @"` "; break;
            }

            this.Editor.Text += text;
            this.Editor.SelectionStart = this.Editor.Text.Length;

            this.FormattingHelperButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.SpoilerButton.Visibility = Windows.UI.Xaml.Visibility.Visible;

            this.Editor.Focus(FocusState.Programmatic);
        }

        private async void HashtagButton_Click(object sender, RoutedEventArgs e)
        {
            var editor = this.Editor;
            if (editor.FocusState == FocusState.Pointer || editor.FocusState == FocusState.Programmatic)
                editor.Focus(FocusState.Unfocused);

            var f = this.Resources["HashtagFlyout"] as Flyout;

            await Task.Delay(200);

            f.ShowAt(this);
        }

        private async void LennysButton_Click(object sender, RoutedEventArgs e)
        {
            var editor = this.Editor;
            if (editor.FocusState == FocusState.Pointer || editor.FocusState == FocusState.Programmatic)
                editor.Focus(FocusState.Unfocused);

            var f = this.Resources["LennysFlyout"] as FlyoutBase;

            await Task.Delay(200);

            f.ShowAt(this);
        }

        private void SpoilerButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Editor.SelectedText.Length > 0)
            {
                var selectedText = this.Editor.SelectedText;
                var newText = "! " + selectedText + " ";

                var start = this.Editor.SelectionStart;
                var length = this.Editor.SelectionLength;
                this.Editor.Text = this.Editor.Text.Remove(start, length);
                this.Editor.Text += newText;

                this.Editor.SelectionStart = this.Editor.Text.Length;
                this.Editor.Focus(FocusState.Programmatic);
            }
            else
            {
                var start = this.Editor.SelectionStart;
                var newText = this.Editor.Text.Insert(start, "! ");

                this.Editor.Text = newText;
                this.Editor.SelectionStart = start + 2;
                this.Editor.Focus(FocusState.Programmatic);
            }
        }

        private void BoldAppBar_Clicked(object sender, RoutedEventArgs e)
        {
            if (this.Editor.SelectedText.Length > 0)
            {
                var selectedText = this.Editor.SelectedText;
                var newText = "**" + selectedText + "** ";

                var start = this.Editor.SelectionStart;
                var length = this.Editor.SelectionLength;
                this.Editor.Text = this.Editor.Text.Remove(start, length);
                this.Editor.Text += newText;

                this.Editor.SelectionStart = this.Editor.Text.Length;
                this.Editor.Focus(FocusState.Programmatic);
            }
            else
            {
                FormattingHelper("bold");
            }
        }

        private void ItalicAppBar_Cliked(object sender, RoutedEventArgs e)
        {
            if (this.Editor.SelectedText.Length > 0)
            {
                var selectedText = this.Editor.SelectedText;
                var newText = "_" + selectedText + "_ ";

                var start = this.Editor.SelectionStart;
                var length = this.Editor.SelectionLength;
                this.Editor.Text = this.Editor.Text.Remove(start, length);
                this.Editor.Text += newText;

                this.Editor.SelectionStart = this.Editor.Text.Length;
                this.Editor.Focus(FocusState.Programmatic);
            }
            else
            {
                FormattingHelper("italic");
            }
        }

        private void CodeAppBar_Click(object sender, RoutedEventArgs e)
        {
            if (this.Editor.SelectedText.Length > 0)
            {
                var selectedText = this.Editor.SelectedText;
                var newText = "`" + selectedText + "` ";

                var start = this.Editor.SelectionStart;
                var length = this.Editor.SelectionLength;
                this.Editor.Text = this.Editor.Text.Remove(start, length);
                this.Editor.Text += newText;

                this.Editor.SelectionStart = this.Editor.Text.Length;
                this.Editor.Focus(FocusState.Programmatic);
            }
            else
            {
                FormattingHelper("code");
            }
        }

        private void QuoteAppBar_Clicked(object sender, RoutedEventArgs e)
        {
            if (this.Editor.SelectedText.Length > 0)
            {
                var selectedText = this.Editor.SelectedText;
                var newText = "> " + selectedText + " ";

                var start = this.Editor.SelectionStart;
                var length = this.Editor.SelectionLength;
                this.Editor.Text = this.Editor.Text.Remove(start, length);
                this.Editor.Text += newText;

                this.Editor.SelectionStart = this.Editor.Text.Length;
                this.Editor.Focus(FocusState.Programmatic);
            }
            else
            {
                this.Editor.Text += "> ";
                this.Editor.SelectionStart = this.Editor.Text.Length;
                this.Editor.Focus(FocusState.Programmatic);
            }
        }

        private void HyperlinkAppBar_Clicked(object sender, RoutedEventArgs e)
        {
            /*
            App.MainViewModel.NewEntryData.body = this.Editor.Text;

            this.Frame.Navigate(typeof(AddEntryHyperlink));
             * */
        }

        private void Editor_Loaded(object sender, RoutedEventArgs e)
        {
            string txt = this.Editor.Text;

            if (txt.Length > 0 /*|| !string.IsNullOrEmpty(this.AttachmentName)*/)
                this.SendButton.IsEnabled = true;
            else
                this.SendButton.IsEnabled = false;
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            string txt = this.Editor.Text;

            if (txt.Length > 0 /*|| !string.IsNullOrEmpty(this.AttachmentName)*/)
                this.SendButton.IsEnabled = true;
            else
                this.SendButton.IsEnabled = false;
        }

        private void HashtagSuggestionBox_HashtagSelected(object sender, StringEventArgs e)
        {
            var tag = e.String;

            AppendTextAndHideFlyout(" " + tag + " ", "HashtagFlyout");

            this.Editor.Focus(FocusState.Programmatic);
        }

        private void AttachmentSymbol_Holding(object sender, HoldingRoutedEventArgs e)
        {
            var mf = this.Resources["DeleteAttachmentFlyout"] as MenuFlyout;
            mf.ShowAt(this.AttachmentSymbol);
        }

        private void AttachmentAppBar_Clicked(object sender, RoutedEventArgs e)
        {
            /*
            App.MainViewModel.NewEntryData.body = this.Editor.Text;

            this.Frame.Navigate(typeof(AddEntryAttachment));*/
        }
    }
}
