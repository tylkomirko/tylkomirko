using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.Controls;
using Mirko_v2.ViewModel;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public sealed partial class ProfilePage : UserControl, IHaveAppBar
    {
        public ProfilePage()
        {
            this.InitializeComponent();
        }

        private CommandBar AppBar = null;
        public CommandBar CreateCommandBar()
        {
            var c = new CommandBar();
            c.SetBinding(CommandBar.VisibilityProperty, new Binding()
            {
                Source = SimpleIoc.Default.GetInstance<ProfilesViewModel>(),
                Path = new PropertyPath("CurrentProfile.Data.Login"),
                Converter = App.Current.Resources["ProfileAppBar"] as IValueConverter,
            });

            var observe = new AppBarToggleButton()
            {
                Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/appbar.eye.png") },
            };
            observe.SetBinding(AppBarToggleButton.IsCheckedProperty, new Binding()
            {
                Source = SimpleIoc.Default.GetInstance<ProfilesViewModel>(),
                Path = new PropertyPath("CurrentProfile.Data.Observed"),
            });
            observe.SetBinding(AppBarToggleButton.LabelProperty, new Binding()
            {
                Source = observe,
                Path = new PropertyPath("IsChecked"),
                Converter = App.Current.Resources["ObservedToText"] as IValueConverter,
            });
            observe.SetBinding(AppBarToggleButton.CommandProperty, new Binding()
            {
                Source = SimpleIoc.Default.GetInstance<ProfilesViewModel>(),
                Path = new PropertyPath("CurrentProfile.Observe"),
            });

            var blacklist = new AppBarToggleButton()
            {
                Icon = new BitmapIcon() { UriSource = new Uri("ms-appx:///Assets/czacha.png") },
            };

            blacklist.SetBinding(AppBarToggleButton.IsCheckedProperty, new Binding()
            {
                Source = SimpleIoc.Default.GetInstance<ProfilesViewModel>(),
                Path = new PropertyPath("CurrentProfile.Data.Blacklisted"),
            });

            blacklist.SetBinding(AppBarToggleButton.LabelProperty, new Binding()
            {
                Source = observe,
                Path = new PropertyPath("IsChecked"),
                Converter = App.Current.Resources["BlacklistToText"] as IValueConverter,
            });

            blacklist.SetBinding(AppBarToggleButton.CommandProperty, new Binding()
            {
                Source = SimpleIoc.Default.GetInstance<ProfilesViewModel>(),
                Path = new PropertyPath("CurrentProfile.Blacklist"),
            });

            var pm = new AppBarButton()
            {
                Label = "wiadomość",
                Icon = new SymbolIcon(Symbol.Message),
            };

            pm.SetBinding(AppBarButton.CommandProperty, new Binding()
            {
                Source = SimpleIoc.Default.GetInstance<ProfilesViewModel>(),
                Path = new PropertyPath("CurrentProfile.PM"),
            });

            c.PrimaryCommands.Add(observe);
            c.PrimaryCommands.Add(blacklist);
            c.PrimaryCommands.Add(pm);

            AppBar = c;
            return AppBar;
        }

        private void ListViewEx_Loaded(object sender, RoutedEventArgs e)
        {
            var lv = sender as ListViewEx;
            lv.AppBar = AppBar;
        }
    }
}
