using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.ViewModel;
using Windows.UI.Xaml.Controls;

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
    }
}
