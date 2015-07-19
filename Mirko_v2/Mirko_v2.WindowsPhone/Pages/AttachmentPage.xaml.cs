using Mirko_v2.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public sealed partial class AttachmentPage : UserControl, IHaveAppBar
    {
        public AttachmentPage()
        {
            this.InitializeComponent();
        }

        public CommandBar CreateCommandBar()
        {
            var ok = new AppBarButton()
            {
                Label = "ok",
                Icon = new SymbolIcon(Symbol.Accept),
            };

            ok.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = this.DataContext as NewEntryViewModel,
                Path = new PropertyPath("AcceptAttachments"),
            });

            var c = new CommandBar();
            c.PrimaryCommands.Add(ok);

            return c;
        }
    }
}
