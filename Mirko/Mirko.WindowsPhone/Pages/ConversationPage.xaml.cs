using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mirko.Utils;
using Mirko.ViewModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using WykopSDK.API.Models;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
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

    public sealed partial class ConversationPage : Page, IReceiveRTBClicks
    {
        public ConversationPage()
        {
            this.InitializeComponent();

            this.ListView.Loaded += async (s, args) =>
            {
                var items = ListView.ItemsSource as ObservableCollectionEx<PMViewModel>;
                if (items != null)
                {
                    if (items.Count > 0)
                    {
                        ListView.ScrollIntoView(items.Last(), ScrollIntoViewAlignment.Leading);
                        await Task.Delay(500);
                        ListView.ScrollIntoView(items.Last(), ScrollIntoViewAlignment.Leading);
                    }
                    else // happens sometimes with push notifications
                        items.CollectionChanged += Messages_CollectionChanged;
                }
            };

            this.TextBox.Loaded += (s, e) => HandleSendButton();
            this.TextBox.TextChanged += (s, e) => HandleSendButton();

            Messenger.Default.Register<NotificationMessage>(this, ReadMessage);
        }

        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                var col = sender as ObservableCollectionEx<PMViewModel>;
                ListView.ScrollIntoView(col.Last(), ScrollIntoViewAlignment.Leading);
                col.CollectionChanged -= Messages_CollectionChanged;
            }
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

        private void LennyChooser_LennySelected(object sender, StringEventArgs e)
        {
            var txt = e.String + " ";
            this.TextBox.Text += txt;
            this.TextBox.SelectionStart = this.TextBox.Text.Length;

            LennyFlyout.Hide();
        }

        private void AttachmentSymbol_Holding(object sender, HoldingRoutedEventArgs e)
        {
            var mf = Resources["DeleteAttachmentFlyout"] as MenuFlyout;
            mf.ShowAt(AttachmentSymbol);
        }

        private void IAP_LayoutChangeCompleted(object sender, QKit.LayoutChangeEventArgs e)
        {
            if (ListView != null && !e.IsDefaultLayout)
                JumpToBottom();
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

        private void ContentRoot_Loaded(object sender, RoutedEventArgs e)
        {
            var height = SimpleIoc.Default.GetInstance<MainViewModel>().ListViewHeaderHeight + 49; // adjust for header
            ContentRoot.Margin = new Thickness(10, -height, 10, 0);

            var header = ListView.Header as FrameworkElement;
            header.Height = height;
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var vm = (this.DataContext as MessagesViewModel).CurrentConversation;
            vm.NewEntry.RemoveAttachment();
            HandleSendButton();
        }

        private void HandleSendButton()
        {
            var vm = (this.DataContext as MessagesViewModel).CurrentConversation;
            if (vm == null)
            {
                this.SendButton.IsEnabled = true;
                return;
            }

            var txt = TextBox.Text;
            var attachmentName = vm.NewEntry.AttachmentName;

            if (txt.Length > 0 || !string.IsNullOrEmpty(attachmentName))
                this.SendButton.IsEnabled = true;
            else
                this.SendButton.IsEnabled = false;
        }

        public void HashtagTapped(string tag, TextBlock tb)
        {
            if (string.IsNullOrEmpty(tag) || tb == null) return;

            var mf = Resources["HashtagFlyout"] as MenuFlyout;
            InjectedRTBHelper.PrepareHashtagFlyout(ref mf, tag);
            var VM = (this.DataContext as MessagesViewModel);
            VM.CurrentConversation.TappedHashtag = tag;

            mf.ShowAt(tb);
        }

        public void ProfileTapped(string username)
        {
            if (string.IsNullOrEmpty(username)) return;
            InjectedRTBHelper.GoToProfilePage(username);
        }
    }
}
