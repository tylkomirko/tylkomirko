using Mirko.ViewModel;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class AttachmentPage : Page
    {
        public AttachmentPage()
        {
            this.InitializeComponent();
            this.URLTextBox.TextChanged += (s, e) => HandleOKButtonVisibility();
            this.Loaded += (s, e) => HandleOKButtonVisibility();
        }

        private void HandleOKButtonVisibility()
        {
            var VM = this.DataContext as NewEntryBaseViewModel;
            var data = VM.NewEntry;

            if (URLTextBox.Text.Length > 0 || data.Files != null)
                OKButton.IsEnabled = true;
            else
                OKButton.IsEnabled = false;
        }
    }
}
