using Mirko_v2.Utils;
using Mirko_v2.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public sealed partial class NewEntryPage : UserControl, IHaveAppBar
    {
        public NewEntryPage()
        {
            this.InitializeComponent();

            this.LayoutRoot.Loaded += (s, e) => FormattingPopup.IsOpen = true;
            this.Editor.Loaded += (s, e) => HandleSendButton();
            this.Editor.TextChanged += (s, e) => HandleSendButton();
        }

        private void ContentRoot_LayoutChangeCompleted(object sender, QKit.LayoutChangeEventArgs e)
        {
            if (e.IsDefaultLayout)
                PageTitle.Visibility = Windows.UI.Xaml.Visibility.Visible;
            else
                PageTitle.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void HandleSendButton()
        {
            string txt = this.Editor.Text;
            var vm = this.DataContext as NewEntryViewModel;
            var attachmentName = vm.Data.AttachmentName;

            if (txt.Length > 0 || !string.IsNullOrEmpty(attachmentName))
                this.SendButton.IsEnabled = true;
            else
                this.SendButton.IsEnabled = false;
        }

        private void AttachmentSymbol_Holding(object sender, HoldingRoutedEventArgs e)
        {
            var mf = this.Resources["DeleteAttachmentFlyout"] as MenuFlyout;
            mf.ShowAt(this.AttachmentSymbol);
        }

        private void RemoveAttachment_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as NewEntryViewModel;
            vm.RemoveAttachment();
            HandleSendButton();
        }

        #region AppBar
        private CommandBar AppBar = null;
        private AppBarButton SendButton = null;
        private AppBarToggleButton FormattingButton = null;

        public CommandBar CreateCommandBar()
        {
            SendButton = new AppBarButton()
            {
                Label = "wyślij",
                Icon = new SymbolIcon(Symbol.Send),
                IsEnabled = false,
            };
            SendButton.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = this.DataContext as NewEntryViewModel,
                Path = new PropertyPath("SendMessageCommand"),
            });

            var lenny = new AppBarButton()
            {
                Label = "lenny",
                Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/appbar.smiley.glasses.png") },
            };
            lenny.Click += async (s, e) =>
            {
                var editor = this.Editor;
                if (editor.FocusState == FocusState.Pointer || editor.FocusState == FocusState.Programmatic)
                    editor.Focus(FocusState.Unfocused);

                var f = this.Resources["LennysFlyout"] as Flyout;

                await Task.Delay(200);

                f.ShowAt(this);
            };

            var attachment = new AppBarButton()
            {
                Label = "załącznik",
                Icon = new SymbolIcon(Symbol.Attach),
            };
            attachment.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = this.DataContext as NewEntryViewModel,
                Path = new PropertyPath("AddAttachment"),
            });

            FormattingButton = new AppBarToggleButton()
            {
                Visibility = Windows.UI.Xaml.Visibility.Collapsed,
            };
            FormattingButton.Click += FormattingButton_Click;

            AppBar = new CommandBar();
            AppBar.PrimaryCommands.Add(SendButton);
            AppBar.PrimaryCommands.Add(lenny);
            AppBar.PrimaryCommands.Add(attachment);
            AppBar.PrimaryCommands.Add(FormattingButton);

            return AppBar;
        }
        #endregion

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
            { FormattingEnum.QUOTE, Tuple.Create<string, string>("\n> ", "\n") },
            { FormattingEnum.SPOILER, Tuple.Create<string, string>("\n! ", "\n") },
        };

        private void AppendTextAndHideFlyout(string text, string flyoutName)
        {
            var start = this.Editor.SelectionStart;
            var newText = this.Editor.Text.Insert(start, text);

            this.Editor.Text = newText;
            this.Editor.SelectionStart = start + newText.Length;

            var f = this.Resources[flyoutName] as FlyoutBase;
            f.Hide();
        }

        private void HyperlinkFlyoutButton_Click(object sender, RoutedEventArgs e)
        {
            string txt = "[" + this.DescriptionTextBox.Text + "](" + this.LinkTextBox.Text + ")";
            AppendTextAndHideFlyout(txt, "HyperlinkFlyout");

            this.DescriptionTextBox.Text = "";
            this.LinkTextBox.Text = "";
        }

        private void LennyChooser_LennySelected(object sender, StringEventArgs e)
        {
            var txt = e.String + " ";
            AppendTextAndHideFlyout(txt, "LennysFlyout");
        }

        private void FormattingPopup_Loaded(object sender, RoutedEventArgs e)
        {
            var popup = sender as Popup;
            if (popup.VerticalOffset == 0)
            {
                var stackPanel = popup.Child as Grid;
                stackPanel.Width = Window.Current.Bounds.Width;
                popup.Width = Window.Current.Bounds.Width;

                popup.VerticalOffset = this.ActualHeight - 2 * stackPanel.Height;
            }
        }

        private void SetFormattingButton()
        {
            string label = "", icon = "";

            if (SelectedFormatting == FormattingEnum.BOLD)
            {
                label = "pogrubienie";
                icon = "appbar.text.bold.png";
            }
            else if (SelectedFormatting == FormattingEnum.ITALIC)
            {
                label = "pochylenie";
                icon = "appbar.text.italic.png";
            }
            else if (SelectedFormatting == FormattingEnum.CODE)
            {                
                label = "kod";
                icon = "appbar.code.xml.png";
            }
            else if(SelectedFormatting == FormattingEnum.SPOILER)
            {
                label = "spoiler";
            }
            else if (SelectedFormatting == FormattingEnum.QUOTE)
            {
                label = "cytat";
                icon = "appbar.quote.png";
            }

            FormattingButton.Label = label;
            if (!string.IsNullOrEmpty(icon))
                FormattingButton.Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/" + icon) };
            else
                FormattingButton.Icon = new FontIcon() { Glyph = "!", FontSize = 28, FontWeight = FontWeights.Bold };
            FormattingButton.IsChecked = true;
            FormattingButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void FormattingChanged(object sender, RoutedEventArgs e)
        {
            var button = sender as AppBarButton;
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

            if(SelectedFormatting == FormattingEnum.LINK)
            {
                var f = Resources["HyperlinkFlyout"] as Flyout;
                f.ShowAt(this);
                return;
            }

            if (this.Editor.SelectedText.Length > 0)
            {
                var selectedText = this.Editor.SelectedText;
                var prefix = FormattingPrefixes[SelectedFormatting].Item1;
                var newText = prefix + selectedText +
                    FormattingPrefixes[SelectedFormatting].Item2;

                var start = this.Editor.SelectionStart;
                var length = this.Editor.SelectionLength;
                var txt = this.Editor.Text.Remove(start, length).Insert(start, newText);
                this.Editor.Text = txt;

                this.Editor.SelectionStart = start + newText.Length;
                this.Editor.Focus(FocusState.Programmatic);

                SelectedFormatting = FormattingEnum.NONE;
            }
            else
            {
                var start = this.Editor.SelectionStart;
                var prefix = FormattingPrefixes[SelectedFormatting].Item1;
                                
                if(SelectedFormatting == FormattingEnum.SPOILER || SelectedFormatting == FormattingEnum.QUOTE)
                {
                    if (this.Editor.Text.Length == 0 || this.Editor.Text.EndsWith("\n"))
                        prefix = prefix.TrimStart(); // remove newline character
                }

                var newText = this.Editor.Text.Insert(start, prefix);

                this.Editor.Text = newText;
                this.Editor.SelectionStart = start + prefix.Length;
                SetFormattingButton();
                this.Editor.Focus(FocusState.Programmatic);
            }
        }

        private void FormattingButton_Click(object sender, RoutedEventArgs e)
        {
            var insertion = FormattingPrefixes[SelectedFormatting].Item2;
            if (this.Editor.SelectedText.Length > 0)
            {
                var selectedText = this.Editor.SelectedText;
                var newText = selectedText + insertion;

                var start = this.Editor.SelectionStart;
                var length = this.Editor.SelectionLength;
                var txt = this.Editor.Text.Remove(start, length).Insert(start, newText);
                this.Editor.Text = txt;

                this.Editor.SelectionStart = start + newText.Length;
            }
            else
            {
                var length = this.Editor.Text.Length;
                this.Editor.Text += insertion;
                this.Editor.SelectionStart = length + insertion.Length;
            }

            SelectedFormatting = FormattingEnum.NONE;
            FormattingButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            this.Editor.Focus(FocusState.Programmatic);
        }

        #endregion
    }
}
