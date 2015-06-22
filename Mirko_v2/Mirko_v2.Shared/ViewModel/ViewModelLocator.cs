/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Mirko_v2"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using Mirko_v2.Pages;

namespace Mirko_v2.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            _navService = new Mirko_v2.ViewModel.NavigationService();
            NavService.RegisterPage("MainPage", typeof(HostPage));
            NavService.RegisterPage("LoginPage", typeof(LoginPage));
            NavService.RegisterPage("EntryPage", typeof(EntryPage));
            NavService.RegisterPage("EmbedPage", typeof(EmbedPage));
            NavService.RegisterPage("SettingsPage", typeof(SettingsPage));
            NavService.RegisterPage("HashtagSelectionPage", typeof(HashtagSelectionPage));
            NavService.RegisterPage("HashtagNotificationsPage", typeof(HashtagNotificationsPage));
            NavService.RegisterPage("HashtagFlipPage", typeof(HashtagFlipPage));
            NavService.RegisterPage("HashtagEntriesPage", typeof(HashtagEntriesPage));
            NavService.RegisterPage("AtNotificationsPage", typeof(AtNotificationsPage));
            NavService.RegisterPage("ConversationsPage", typeof(ConversationsPage));
            NavService.RegisterPage("ConversationPage", typeof(ConversationPage));
            NavService.RegisterPage("AddAttachmentPage", typeof(AddAttachmentPage));

            NavService.RegisterPage("PivotPage", typeof(PivotPage));
            SimpleIoc.Default.Register<GalaSoft.MvvmLight.Views.INavigationService>(() => NavService);

            SimpleIoc.Default.Register<MainViewModel>(() => { return new MainViewModel(NavService); });
            SimpleIoc.Default.Register<LoginViewModel>();
            SimpleIoc.Default.Register<PaymentViewModel>(true);
            SimpleIoc.Default.Register<SettingsViewModel>(true);
            SimpleIoc.Default.Register<FontsViewModel>();
            SimpleIoc.Default.Register<NotificationsViewModel>(() => { return new NotificationsViewModel(NavService); });
            SimpleIoc.Default.Register<CacheViewModel>();
            SimpleIoc.Default.Register<MessagesViewModel>(() => { return new MessagesViewModel(NavService); });
            SimpleIoc.Default.Register<AddEntryViewModel>();
        }

        private NavigationService _navService = null;
        public NavigationService NavService
        {
            get { return _navService; } 
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        public NotificationsViewModel Notifications
        {
            get { return ServiceLocator.Current.GetInstance<NotificationsViewModel>(); }
        }

        public MessagesViewModel Messages
        {
            get { return ServiceLocator.Current.GetInstance<MessagesViewModel>(); }
        }

        public AddEntryViewModel AddEntry
        {
            get { return ServiceLocator.Current.GetInstance<AddEntryViewModel>(); }
        }

        public CacheViewModel Cache
        {
            get { return ServiceLocator.Current.GetInstance<CacheViewModel>(); }
        }

        public SettingsViewModel Settings
        {
            get { return ServiceLocator.Current.GetInstance<SettingsViewModel>(); }
        }

        public LoginViewModel Login
        {
            get { return ServiceLocator.Current.GetInstance<LoginViewModel>(); }
        }

        public FontsViewModel Fonts
        {
            get { return ServiceLocator.Current.GetInstance<FontsViewModel>(); }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}