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

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<LoginViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<FontsViewModel>();
            SimpleIoc.Default.Register<NotificationsViewModel>();
            SimpleIoc.Default.Register<CacheViewModel>();
            SimpleIoc.Default.Register<MessagesViewModel>();
            SimpleIoc.Default.Register<AddEntryViewModel>();
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