using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Mirko.Utils;
using Mirko.ViewModel;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class SettingsPage : UserControl, IHaveAppBar
    {
        private SettingsViewModel VM
        {
            get { return DataContext as SettingsViewModel; }
        }

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

            c.SecondaryCommands.Add(debug);

            return c;
        }
    }
}
