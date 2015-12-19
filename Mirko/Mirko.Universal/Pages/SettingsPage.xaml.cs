using GalaSoft.MvvmLight.Ioc;
using Mirko.Utils;
using Mirko.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class SettingsPage : Page
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
                    if (App.IsMobile)
                        StatusBarManager.HideStatusBar();

                    if (VM.SelectedTheme == ElementTheme.Dark)
                        NightMode.IsChecked = true;
                    else
                        DayMode.IsChecked = true;

                    DayMode.Checked += (se, args) => { VM.SelectedTheme = ElementTheme.Light; ApplyBackgroundBrush(); };
                    NightMode.Checked += (se, args) => { VM.SelectedTheme = ElementTheme.Dark; ApplyBackgroundBrush(); };

                    ApplyBackgroundBrush();
                };

            if (App.IsMobile)
                this.Unloaded += (s, e) => StatusBarManager.ShowStatusBar();
        }

        private void ApplyBackgroundBrush()
        {
            string brushKey = null;
            if (NightMode.IsChecked.Value)
                brushKey = "SettingsBackgroundDark";
            else
                brushKey = "SettingsBackgroundLight";

            LayoutRoot.Background = Application.Current.Resources[brushKey] as SolidColorBrush;
        }
    }
}
