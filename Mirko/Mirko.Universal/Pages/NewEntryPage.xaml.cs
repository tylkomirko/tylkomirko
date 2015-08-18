using Mirko.Utils;
using Mirko.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class NewEntryPage : UserControl
    {
        private NewEntryBaseViewModel VM
        {
            get { return DataContext as NewEntryBaseViewModel; }
        }

        public NewEntryPage()
        {
            this.InitializeComponent();

            if (App.IsMobile)
            {
                FormattingAppBar.MakeButtonInvisible("code");
                FormattingAppBar.MakeButtonInvisible("quote");
            }
            else
            {
                FormattingAppBar.SecondaryCommands.Clear();
            }
        }
        
        private TextBox CurrentEditor()
        {
            var item = FlipView.ContainerFromIndex(FlipView.SelectedIndex) as FlipViewItem;
            var grid = item.ContentTemplateRoot as Grid;
            return grid.FindName("Editor") as TextBox;
        }

        private ScrollViewer CurrentEntryPreview()
        {
            var item = FlipView.ContainerFromIndex(FlipView.SelectedIndex) as FlipViewItem;
            var grid = item.ContentTemplateRoot as Grid;
            return grid.FindName("EntryPreview") as ScrollViewer;
        }

        private TextBlock CurrentAttachmentSymbol()
        {
            var item = FlipView.ContainerFromIndex(FlipView.SelectedIndex) as FlipViewItem;
            var grid = item.ContentTemplateRoot as Grid;
            return grid.FindName("AttachmentSymbol") as TextBlock;
        }

        private Rectangle CurrentFooter()
        {
            var item = FlipView.ContainerFromIndex(FlipView.SelectedIndex) as FlipViewItem;
            var grid = item.ContentTemplateRoot as Grid;
            return grid.FindName("Footer") as Rectangle;
        }

        private void PageTitle_Loaded(object sender, RoutedEventArgs e)
        {
            var data = (this.DataContext as NewEntryViewModel).NewEntry;

            string title;
            if(data.IsEditing)
            {
                title = "edytujesz ";
                title += data.CommentID == 0 ? "wpis" : "komentarz";
            }
            else
            {
                title = "nowy ";
                title += data.EntryID == 0 ? "wpis" : "komentarz";
            }

            PageTitle.Text = title;
        }

        private void HandleSendButton()
        {
            string txt = CurrentEditor().Text;
            var vm = this.DataContext as NewEntryViewModel;
            var attachmentName = vm.NewEntry.AttachmentName;

            if (txt.Length > 0 || !string.IsNullOrEmpty(attachmentName))
                this.SendButton.IsEnabled = true;
            else
                this.SendButton.IsEnabled = false;
        }

        private void AttachmentSymbol_OpenFlyout(object sender, RoutedEventArgs e)
        {
            if (e is HoldingRoutedEventArgs || e is RightTappedRoutedEventArgs)
            {
                var attachmentTB = CurrentAttachmentSymbol();
                FlyoutBase.ShowAttachedFlyout(attachmentTB);
            }
        }

        private void RemoveAttachment_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as NewEntryViewModel;
            vm.NewEntry.RemoveAttachment();
            HandleSendButton();
        }

        #region Formatting
        internal enum FormattingEnum
        {
            NONE,
            BOLD,
            ITALIC,
            CODE,
            SPOILER,
            LINK,
            QUOTE
        };

        private FormattingEnum SelectedFormatting;
        private readonly Dictionary<FormattingEnum, Tuple<string, string>> FormattingPrefixes = new Dictionary<FormattingEnum, Tuple<string, string>>()
        {
            { FormattingEnum.BOLD, Tuple.Create<string, string>("**", "** ") },
            { FormattingEnum.ITALIC, Tuple.Create<string, string>("_", "_ ") },
            { FormattingEnum.CODE, Tuple.Create<string, string>("`", "` ") },
            { FormattingEnum.QUOTE, Tuple.Create<string, string>("\r\n> ", "\r\n") },
            { FormattingEnum.SPOILER, Tuple.Create<string, string>("\r\n! ", "\r\n") },
        };

        private void InsertTextAndHideFlyout(string text, string flyoutName)
        {
            var start = CurrentEditor().GetNormalizedSelectionStart();
            var newText = CurrentEditor().Text.Insert(start, text);

            CurrentEditor().Text = newText;
            CurrentEditor().SelectionStart = start + newText.Length;

            if (Resources.ContainsKey(flyoutName))
            {
                var f = this.Resources[flyoutName] as FlyoutBase;
                if(f != null)
                    f.Hide();
            }
        }

        private void HyperlinkFlyoutButton_Click(object sender, RoutedEventArgs e)
        {
            string txt = "[" + this.DescriptionTextBox.Text + "](" + this.LinkTextBox.Text + ")";
            InsertTextAndHideFlyout(txt, "HyperlinkFlyout");
            HyperlinkFlyout.Hide();

            this.DescriptionTextBox.Text = "";
            this.LinkTextBox.Text = "";
        }

        private void LennyChooser_LennySelected(object sender, StringEventArgs e)
        {
            var txt = e.String + " ";
            InsertTextAndHideFlyout(txt, "LennysFlyout");
            LennyFlyout.Hide();
        }

        private void SetFormattingButton()
        {
            string label = "", icon = "";

            if (SelectedFormatting == FormattingEnum.BOLD)
            {
                label = "pogrubienie";
                FormattingButton.Icon = new SymbolIcon(Symbol.Bold);
            }
            else if (SelectedFormatting == FormattingEnum.ITALIC)
            {
                label = "pochylenie";
                FormattingButton.Icon = new SymbolIcon(Symbol.Italic);
            }
            else if (SelectedFormatting == FormattingEnum.CODE)
            {                
                label = "kod";
                icon = "appbar.code.xml.png";
            }
            else if(SelectedFormatting == FormattingEnum.SPOILER)
            {
                label = "spoiler";
                FormattingButton.Icon = new SymbolIcon(Symbol.Important);
            }
            else if (SelectedFormatting == FormattingEnum.QUOTE)
            {
                label = "cytat";
                icon = "appbar.quote.png";
            }

            FormattingButton.Label = label;
            if (!string.IsNullOrEmpty(icon))
                FormattingButton.Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/" + icon) };
            FormattingButton.IsChecked = true;
            FormattingButton.Visibility = Visibility.Visible;
        }

        private void FormattingChanged(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            var tag = button.Tag as string;

            if (tag == "bold")
                SelectedFormatting = FormattingEnum.BOLD;
            else if (tag == "italic")
                SelectedFormatting = FormattingEnum.ITALIC;
            else if (tag == "code")
                SelectedFormatting = FormattingEnum.CODE;
            else if (tag == "spoiler")
                SelectedFormatting = FormattingEnum.SPOILER;
            else if (tag == "link")
                SelectedFormatting = FormattingEnum.LINK;
            else if (tag == "quote")
                SelectedFormatting = FormattingEnum.QUOTE;

            var editor = CurrentEditor();

            if(SelectedFormatting == FormattingEnum.LINK)
            {
                FlyoutBase.ShowAttachedFlyout(button);
                return;
            }
            else if (SelectedFormatting == FormattingEnum.QUOTE)
            {
                var VM = this.DataContext as NewEntryViewModel;
                var currentVM = VM.Responses[FlipView.SelectedIndex];
                if(!string.IsNullOrEmpty(currentVM.SelectedText))
                {
                    var insertion = "\n> " + currentVM.SelectedText + "\n";
                    editor.Text += insertion;

                    currentVM.SelectedText = null;
                    SelectedFormatting = FormattingEnum.NONE;
                    return;
                }
            }

            var start = editor.GetNormalizedSelectionStart();

            var prefix = FormattingPrefixes[SelectedFormatting].Item1;
            if (SelectedFormatting == FormattingEnum.SPOILER || SelectedFormatting == FormattingEnum.QUOTE)
            {
                if (editor.Text.Length == 0 || editor.Text.EndsWith("\r\n"))
                    prefix = prefix.TrimStart(); // remove newline character
            }

            if (editor.SelectedText.Length > 0)
            {
                var selectedText = editor.SelectedText;
                var newText = prefix + selectedText +
                    FormattingPrefixes[SelectedFormatting].Item2;

                var length = editor.SelectionLength;
                var txt = editor.Text.Remove(start, length).Insert(start, newText);
                editor.Text = txt;

                editor.SelectionStart = start + newText.Length;
                editor.Focus(FocusState.Programmatic);

                SelectedFormatting = FormattingEnum.NONE;
            }
            else
            {
                var newText = editor.Text.Insert(start, prefix);

                editor.Text = newText;
                editor.SelectionStart = start + prefix.Length;
                SetFormattingButton();
                editor.Focus(FocusState.Programmatic);
            }
        }

        private void FormattingButton_Click(object sender, RoutedEventArgs e)
        {
            var editor = CurrentEditor();
            var insertion = FormattingPrefixes[SelectedFormatting].Item2;

            if (editor.SelectedText.Length > 0)
            {
                var selectedText = editor.SelectedText;
                var newText = selectedText + insertion;

                var start = editor.SelectionStart;
                var length = editor.SelectionLength;
                var txt = editor.Text.Remove(start, length).Insert(start, newText);
                editor.Text = txt;

                editor.SelectionStart = start + newText.Length;
            }
            else
            {
                var length = editor.Text.Length;
                editor.Text += insertion;
                editor.SelectionStart = length + insertion.Length;
            }

            SelectedFormatting = FormattingEnum.NONE;
            FormattingButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            editor.Focus(FocusState.Programmatic);
        }

        #endregion

        private void Editor_Loaded(object sender, RoutedEventArgs e)
        {
            HandleSendButton();
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandleSendButton();
        }

        private void Preview_TextSelectionChanged(object sender, StringEventArgs e)
        {
            var VM = this.DataContext as NewEntryViewModel;
            var currentVM = VM.Responses[FlipView.SelectedIndex];
            currentVM.SelectedText = e.String;
        }

        private void Editor_GotFocus(object sender, RoutedEventArgs e)
        {
            CurrentEntryPreview().Visibility = Visibility.Collapsed;
        }

        private void Editor_LostFocus(object sender, RoutedEventArgs e)
        {
            var VM = this.DataContext as NewEntryViewModel;
            if (VM == null) return;

            var currentVM = VM.Responses[FlipView.SelectedIndex];
            if (currentVM.Preview != null)
                CurrentEntryPreview().Visibility = Visibility.Visible;
        }
    }
}
