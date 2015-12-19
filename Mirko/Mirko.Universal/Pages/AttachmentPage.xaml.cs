using GalaSoft.MvvmLight.Ioc;
using Mirko.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class AttachmentPage : Page
    {
        private NewEntryBaseViewModel VM
        {
            get { return DataContext as NewEntryBaseViewModel; }
        }

        public AttachmentPage()
        {
            this.InitializeComponent();
            this.URLTextBox.TextChanged += (s, e) => HandleOKButtonVisibility();
            this.Loaded += (s, e) =>
            {
                HandleOKButtonVisibility();

                string brushKey = null;
                if (SimpleIoc.Default.GetInstance<SettingsViewModel>().SelectedTheme == ElementTheme.Dark)
                    brushKey = "NewEntryBackgroundDark";
                else
                    brushKey = "NewEntryBackgroundLight";

                LayoutRoot.Background = Application.Current.Resources[brushKey] as SolidColorBrush;
            };
        }

        private void HandleOKButtonVisibility()
        {
            if (VM == null) return;

            var data = VM.NewEntry;
            if (URLTextBox.Text.Length > 0 || data.Files != null)
                OKButton.IsEnabled = true;
            else
                OKButton.IsEnabled = false;
        }
    }
}
