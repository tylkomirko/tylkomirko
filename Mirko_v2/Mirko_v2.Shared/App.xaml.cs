using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using Mirko_v2.Pages;
using Mirko_v2.Utils;
using Mirko_v2.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Mirko_v2
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
        private ContinuationManager continuationManager;
#endif

        private static WykopAPI.WykopAPI _apiService = null;
        public static WykopAPI.WykopAPI ApiService
        {
            get
            {
                if(_apiService == null)
                {
                    _apiService = new WykopAPI.WykopAPI();
                    _apiService.NetworkStatusChanged += ApiService_NetworkStatusChanged;
                    _apiService.MessageReceiver += ApiService_MessageReceiver;
                }
                return _apiService;
            }
        }

        static void ApiService_MessageReceiver(object sender, WykopAPI.MessageEventArgs e)
        {
            
        }

        static async void ApiService_NetworkStatusChanged(object sender, WykopAPI.NetworkEventArgs e)
        {
            if(!e.IsNetworkAvailable)
            {
                await StatusBarManager.ShowText("Brak połączenia z Internetem.");
            }
        }

        public static bool IsWIFIAvailable { get; set; }
        public static bool IsNetworkAvailable { get; set; }
        private Mirko_v2.ViewModel.NavigationService NavService = null;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;

            if (Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey("NightMode"))
            {
                bool nightMode = (bool)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["NightMode"];
                RequestedTheme = nightMode ? ApplicationTheme.Dark : ApplicationTheme.Light;
            }
        }

        private Frame CreateRootFrame()
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content, 
            // just ensure that the window is active 
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page 
                rootFrame = new Frame();

                // Set the default language 
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window 
                Window.Current.Content = rootFrame;
            }

            return rootFrame;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }


        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;
            DispatcherHelper.Initialize();

            // expand window size 
            var applicationView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            applicationView.SetDesiredBoundsMode(Windows.UI.ViewManagement.ApplicationViewBoundsMode.UseCoreWindow);
            StatusBarManager.Init();

            var locator = this.Resources["Locator"] as ViewModelLocator;
            NavService = locator.NavService;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = CreateRootFrame();              

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    await ResumeFromSuspension();
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                SimpleIoc.Default.GetInstance<CacheViewModel>().InitCommand.Execute(null);
                App.ApiService.LocalStorage.InitAction();

                if (!string.IsNullOrEmpty(e.Arguments))
                    ProcessLaunchArguments(e.Arguments);
                else
                    NavService.NavigateTo("PivotPage");
            }
            else
            {
                ProcessLaunchArguments(e.Arguments);
            }

            // Ensure the current window is active
            Window.Current.Activate();

            if (!Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("FirstRun"))
            {
                await BackgroundTasksUtils.RegisterTask(typeof(BackgroundTasks.Cleaner).FullName,
                    "Cleaner",
                    new MaintenanceTrigger(60 * 24, false),
                    new SystemCondition(SystemConditionType.UserNotPresent));

                Windows.Storage.ApplicationData.Current.LocalSettings.Values["FirstRun"] = false;
            }
        }

        private void ProcessLaunchArguments(string args)
        {
            if (string.IsNullOrEmpty(args)) return;

            NavService.InsertMainPage();

            var notification = JsonConvert.DeserializeObject<WykopAPI.Models.Notification>(args);
            if(notification.Type == WykopAPI.Models.NotificationType.EntryDirected || notification.Type == WykopAPI.Models.NotificationType.CommentDirected)
            {
                var VM = SimpleIoc.Default.GetInstance<NotificationsViewModel>(); // make sure it exists
                Messenger.Default.Send<NotificationMessage<WykopAPI.Models.Notification>>
                    (new NotificationMessage<WykopAPI.Models.Notification>(notification, "Go to"));
            }
            else if(notification.Type == WykopAPI.Models.NotificationType.PM)
            {
                var VM = SimpleIoc.Default.GetInstance<MessagesViewModel>(); // make sure it exists
                Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(notification.AuthorName, "Go to"));
            }
            else
            {
                NavService.NavigateTo("AtNotificationsPage");
            }
        }

#if WINDOWS_PHONE_APP
        private async Task RestoreStatusAsync(ApplicationExecutionState previousExecutionState)
        {
            // Do not repeat app initialization when the Window already has content, 
            // just ensure that the window is active 
            if (previousExecutionState == ApplicationExecutionState.Terminated)
            {
                // Restore the saved session state only when appropriate 
                try
                {
                    await SuspensionManager.RestoreAsync();
                }
                catch (SuspensionManagerException)
                {
                    //Something went wrong restoring state. 
                    //Assume there is no state and continue 
                }
            }
        } 

        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection();// { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        /// <summary> 
        /// Handle OnActivated event to deal with File Open/Save continuation activation kinds 
        /// </summary> 
        /// <param name="e">Application activated event arguments, it can be casted to proper sub-type based on ActivationKind</param> 
        protected async override void OnActivated(IActivatedEventArgs e)
        {
            base.OnActivated(e);

            continuationManager = new ContinuationManager();

            Frame rootFrame = CreateRootFrame();
            await RestoreStatusAsync(e.PreviousExecutionState);

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(HostPage));
            }

            var continuationEventArgs = e as IContinuationActivatedEventArgs;
            if (continuationEventArgs != null)
            {
                // Call ContinuationManager to handle continuation activation 
                continuationManager.Continue(continuationEventArgs);
            }

            Window.Current.Activate();
        } 
#endif

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            var currentPage = NavService.CurrentPageKey;
            var currentFrame = NavService.CurrentFrame();
            if (currentFrame.DataContext is IResumable)
            {
                var resumableVM = currentFrame.DataContext as IResumable;
                await resumableVM.SaveState(currentPage);

                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings.Values;
                localSettings["PageKey"] = currentPage;
                localSettings["VM"] = resumableVM.GetName();
            }

            deferral.Complete();
        }

        private async Task ResumeFromSuspension()
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings.Values;
            if (!settings.ContainsKey("PageKey")) return;

            var pageKey = (string)settings["PageKey"];
            var viewModelName = (string)settings["VM"];

            bool resumed = false;
            if (viewModelName == "MainViewModel")
            {
                var resumableVM = SimpleIoc.Default.GetInstance<MainViewModel>() as IResumable;
                resumed = await resumableVM.LoadState(pageKey);
            }

            if(resumed)
            {
                NavService.InsertMainPage();
                NavService.NavigateTo(pageKey);
            }
            else
            {
                NavService.NavigateTo("PivotPage");
            }
        }
    }
}