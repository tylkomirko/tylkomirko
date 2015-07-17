using GalaSoft.MvvmLight.Messaging;
using Mirko_v2.Utils;
using Mirko_v2.ViewModel;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
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

    public sealed partial class ConversationPage : UserControl, IHaveAppBar
    {
        public ConversationPage()
        {
            this.InitializeComponent();

            this.ListView.Loaded += (s, args) =>
            {
                if (this.ListView.ItemsSource != null && this.ListView.Items.Count > 0)
                    this.ListView.ScrollIntoView(this.ListView.Items.Last(), ScrollIntoViewAlignment.Leading);
            };

            Messenger.Default.Register<NotificationMessage>(this, ReadMessage);
        }

        private void ReadMessage(NotificationMessage obj)
        {
            if(obj.Notification == "PM-Success")
            {
                TextBox.Text = "";
            }
            else if(obj.Notification == "PM-Fail")
            {
                SendButton.IsEnabled = true;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            AppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.TextBox.Text.Length > 0)
                AppBar.IsSticky = true;
            else
                AppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = TextBox.Text;
            if (txt.Length > 0 || AttachmentSymbol.Text.Length > 3) // 3 is the length of attachment symbol and two spaces
                SendButton.IsEnabled = true;
            else
                SendButton.IsEnabled = false;
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

        #region AppBar
        private CommandBar AppBar = null;
        private AppBarButton SendButton = null;

        public CommandBar CreateCommandBar()
        {
            var c = new CommandBar();

            SendButton = new AppBarButton()
            {
                Label = "wyślij",
                Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/reply.png") },
                IsEnabled = false,
            };

            SendButton.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = this.DataContext as MessagesViewModel,
                Path = new PropertyPath("CurrentConversation.SendMessageCommand"),
            });

            SendButton.Click += (s, e) =>
            {
                SendButton.IsEnabled = false;
            };

            var lenny = new AppBarButton()
            {
                Label = "lenny",
                Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/appbar.smiley.glasses.png") },
            };
            lenny.Click += (s, e) =>
            {
                var flyout = Resources["LennysFlyout"] as Flyout;
                flyout.ShowAt(this);
            };

            c.PrimaryCommands.Add(SendButton);
            c.PrimaryCommands.Add(lenny);

            AppBar = c;
            return AppBar;
        }
        #endregion

        private void IAP_LayoutChangeCompleted(object sender, QKit.LayoutChangeEventArgs e)
        {
            if (ListView != null && !e.IsDefaultLayout)
            {
                JumpToBottom();
            }
        }

        private void JumpToBottom()
        {
            var sv = ListView.GetDescendant<ScrollViewer>();
            if (sv != null)
            {
                sv.UpdateLayout();
                if (sv != null)
                    sv.ChangeView(null, sv.ScrollableHeight, null, true);
                sv.UpdateLayout();
            }

        }
    }
}
