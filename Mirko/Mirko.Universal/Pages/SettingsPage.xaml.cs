using GalaSoft.MvvmLight.Ioc;
using Mirko.Utils;
using Mirko.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class SettingsPage : UserControl
    {
        private SettingsViewModel VM
        {
            get { return DataContext as SettingsViewModel; }
        }

        private MainViewModel MainVM
        {
            get { return SimpleIoc.Default.GetInstance<MainViewModel>(); }
        }

        public SettingsPage()
        {
            this.InitializeComponent();

            this.Loaded += (s, e) =>
            {
                if(App.IsMobile)
                    StatusBarManager.HideStatusBar();

                if (VM.SelectedTheme == ElementTheme.Dark)
                    NightMode.IsChecked = true;
                else
                    DayMode.IsChecked = true;

                DayMode.Checked += (se, args) => VM.SelectedTheme = ElementTheme.Light;
                NightMode.Checked += (se, args) => VM.SelectedTheme = ElementTheme.Dark;
            };

            if(App.IsMobile)
                this.Unloaded += (s, e) => StatusBarManager.ShowStatusBar();
        }
    }
}
