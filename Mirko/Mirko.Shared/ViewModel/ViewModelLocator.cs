/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Mirko"
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
using Mirko.Pages;

namespace Mirko.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private static bool initialized = false; // fuck you ShareTarget

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            if (initialized)
                return;

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

            _navService = new Mirko.ViewModel.NavigationService();
            NavService.RegisterPage("MainPage", typeof(HostPage));
#if WINDOWS_UWP
            NavService.RegisterPage("DualMainPage", typeof(DualHostPage));
#endif
            NavService.RegisterPage("LoginPage", typeof(LoginPage));
            NavService.RegisterPage("EntryPage", typeof(EntryPage));
            NavService.RegisterPage("EmbedPage", typeof(EmbedPage));
            NavService.RegisterPage("SettingsPage", typeof(SettingsPage));
            NavService.RegisterPage("HashtagSelectionPage", typeof(HashtagSelectionPage));
            NavService.RegisterPage("HashtagFlipPage", typeof(HashtagFlipPage));
            NavService.RegisterPage("HashtagEntriesPage", typeof(HashtagEntriesPage));
            NavService.RegisterPage("AtNotificationsPage", typeof(AtNotificationsPage));
            NavService.RegisterPage("ConversationsPage", typeof(ConversationsPage));
            NavService.RegisterPage("DebugPage", typeof(DebugPage));
            NavService.RegisterPage("ProfilePage", typeof(ProfilePage));
            NavService.RegisterPage("AttachmentPage", typeof(AttachmentPage));
            NavService.RegisterPage("ConversationPage", typeof(ConversationPage));
            NavService.RegisterPage("HashtagNotificationsPage", typeof(HashtagNotificationsPage));
            NavService.RegisterPage("NewEntryPage", typeof(NewEntryPage));
            NavService.RegisterPage("PivotPage", typeof(PivotPage));
            NavService.RegisterPage("DonationPage", typeof(DonationPage));
            NavService.RegisterPage("BlacklistPage", typeof(BlacklistPage));
#if WINDOWS_UWP
            NavService.RegisterPage("EmptyPage", typeof(EmptyPage));
#endif
            SimpleIoc.Default.Register<NavigationService>(() => NavService);

            SimpleIoc.Default.Register<MainViewModel>(() => { return new MainViewModel(NavService); });
            SimpleIoc.Default.Register<LoginViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>(true);
            SimpleIoc.Default.Register<FontsViewModel>();
            SimpleIoc.Default.Register<NotificationsViewModel>(() => { return new NotificationsViewModel(NavService); });
            SimpleIoc.Default.Register<CacheViewModel>();
            SimpleIoc.Default.Register<MessagesViewModel>(() => { return new MessagesViewModel(NavService); });
            SimpleIoc.Default.Register<AddEntryViewModel>();
            SimpleIoc.Default.Register<DebugViewModel>(true);
            SimpleIoc.Default.Register<ProfilesViewModel>(() => { return new ProfilesViewModel(NavService); });
            SimpleIoc.Default.Register<NewEntryViewModel>(() => { return new NewEntryViewModel(NavService); });
            SimpleIoc.Default.Register<BlacklistViewModel>(true);

            initialized = true;
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

        public DebugViewModel Debug
        {
            get { return ServiceLocator.Current.GetInstance<DebugViewModel>(); }
        }

        public ProfilesViewModel Profiles
        {
            get { return ServiceLocator.Current.GetInstance<ProfilesViewModel>(); }
        }

        public NewEntryViewModel NewEntry
        {
            get { return ServiceLocator.Current.GetInstance<NewEntryViewModel>(); }
        }

        public BlacklistViewModel Blacklist
        {
            get {return ServiceLocator.Current.GetInstance<BlacklistViewModel>(); }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}