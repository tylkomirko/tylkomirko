using Mirko.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class AttachmentPage : UserControl, IHaveAppBar
    {
        public AttachmentPage()
        {
            this.InitializeComponent();
            this.URLTextBox.TextChanged += (s, e) => HandleOKButtonVisibility();
            this.Loaded += (s, e) => HandleOKButtonVisibility();
        }

        private AppBarButton OKButton;

        public CommandBar CreateCommandBar()
        {
            OKButton = new AppBarButton()
            {
                Label = "ok",
                Icon = new SymbolIcon(Symbol.Accept),
            };

            OKButton.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = this.DataContext as NewEntryBaseViewModel,
                Path = new PropertyPath("AcceptAttachments"),
            });

            var c = new CommandBar();
            c.PrimaryCommands.Add(OKButton);

            return c;
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
