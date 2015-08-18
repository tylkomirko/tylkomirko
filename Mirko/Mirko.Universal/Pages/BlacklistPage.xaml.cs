using Mirko.ViewModel;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class BlacklistPage : UserControl
    {
        private BlacklistViewModel VM
        {
            get { return DataContext as BlacklistViewModel; }
        }

        public BlacklistPage()
        {
            this.InitializeComponent();
        }
    }
}
