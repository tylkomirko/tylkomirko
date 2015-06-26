using Mirko_v2.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public sealed partial class DebugPage : UserControl, IHaveAppBar
    {
        public DebugPage()
        {
            this.InitializeComponent();
        }

        public CommandBar CreateCommandBar()
        {
            var c = new CommandBar();

            var share = new AppBarButton()
            {
                Icon = new SymbolIcon(Symbol.ReShare),
                Label = "udostępnij logi",
            };
            share.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = this.DataContext as DebugViewModel,
                Path = new PropertyPath("ShareCommand"),
            });

            c.PrimaryCommands.Add(share);

            return c;
        }
    }
}
