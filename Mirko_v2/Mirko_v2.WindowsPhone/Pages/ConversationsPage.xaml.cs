using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Mirko_v2.Utils;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public sealed partial class ConversationsPage : UserControl
    {
        public ConversationsPage()
        {
            this.InitializeComponent();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var newItem = e.ClickedItem as ConversationViewModel;
            if (newItem == null) return;

            var VM = SimpleIoc.Default.GetInstance<MessagesViewModel>();
            VM.CurrentConversation = newItem;
            VM.GoToConversationPageCommand.Execute(null);
        }

        private void ListView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var height = SimpleIoc.Default.GetInstance<MainViewModel>().ListViewHeaderHeight;
            ListView.Margin = new Thickness(10, -height, 0, 0);

            var sp = ListView.Header as StackPanel;
            var rect = sp.GetDescendant<Rectangle>();
            rect.Height = height;
        }
    }
}
