using GalaSoft.MvvmLight.Ioc;
using Mirko.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Mirko.Utils;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class ConversationsPage : UserControl
    {
        private MessagesViewModel VM
        {
            get { return DataContext as MessagesViewModel; }
        }

        public ConversationsPage()
        {
            this.InitializeComponent();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var newItem = e.ClickedItem as ConversationViewModel;
            if (newItem == null) return;

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
