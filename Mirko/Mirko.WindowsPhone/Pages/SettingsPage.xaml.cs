using GalaSoft.MvvmLight.Ioc;
using Mirko.Utils;
using Mirko.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class SettingsPage : UserControl, IHaveAppBar
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            this.Loaded += async (s, e) =>
            {
                await StatusBarManager.HideStatusBarAsync();
                var VM = this.DataContext as SettingsViewModel;

                if (VM.SelectedTheme == ElementTheme.Dark)
                    NightMode.IsChecked = true;
                else
                    DayMode.IsChecked = true;

                DayMode.Checked += (se, args) => VM.SelectedTheme = ElementTheme.Light;
                NightMode.Checked += (se, args) => VM.SelectedTheme = ElementTheme.Dark;
            };

            this.Unloaded += async (s, e) => await StatusBarManager.ShowStatusBarAsync();
        }

        public CommandBar CreateCommandBar()
        {
            var c = new CommandBar() { ClosedDisplayMode = AppBarClosedDisplayMode.Minimal };

            var debug = new AppBarButton()
            {
                Label = "debug",
            };
            debug.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = SimpleIoc.Default.GetInstance<MainViewModel>(),
                Path = new PropertyPath("GoToDebugPage"),
            });

            var openURI = new AppBarButton()
            {
                Label = "jak otwierać linki?",
            };
            openURI.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = SimpleIoc.Default.GetInstance<MainViewModel>(),
                Path = new PropertyPath("HowToOpenUris"),
            });

            c.SecondaryCommands.Add(debug);
            c.SecondaryCommands.Add(openURI);

            return c;
        }
    }
}
