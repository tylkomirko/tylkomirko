using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MetroLog;
using MetroLog.Targets;
using Microsoft.ApplicationInsights;
using Mirko.Utils;
using Mirko.ViewModel;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WykopSDK.API;
using WykopSDK.API.Models;
using WykopSDK.Utils;
using WykopSDK.WWW;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Mirko
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

        private static WykopAPI _apiService = null;
        public static WykopAPI ApiService
        {
            get
            {
                if(_apiService == null)
                {
                    _apiService = new WykopAPI();
                    _apiService.NetworkStatusChanged += ApiService_NetworkStatusChanged;
                    _apiService.MessageReceiver += ApiService_MessageReceiver;
                }
                return _apiService;
            }
        }

        static async void ApiService_MessageReceiver(object sender, MessageEventArgs e)
        {
            if (e.Code == 61)
                await StatusBarManager.ShowTextAsync(e.Message);
        }

        static async void ApiService_NetworkStatusChanged(object sender, NetworkEventArgs e)
        {
            if(!e.IsNetworkAvailable)
            {
                await StatusBarManager.ShowTextAsync("Brak połączenia z Internetem.");
            }
        }

        private static WykopWWW _wwwService = null;
        public static WykopWWW WWWService
        {
            get { return _wwwService ?? (_wwwService = new WykopWWW()); }
        }

        private static TelemetryClient _telemetryClient = null;
        public static TelemetryClient TelemetryClient
        {
            get { return _telemetryClient ?? (_telemetryClient = new TelemetryClient()); }
        }

        public static bool IsMobile { get; set; }
        public static bool IsWIFIAvailable { get; set; }
        public static bool IsNetworkAvailable { get; set; }
        public static bool ShareTargetActivated { get; set; }
        private NavigationService NavService = null;
        private readonly ILogger Logger = null;

        private static TimeSpan _offsetUTCInPoland;
        public static TimeSpan OffsetUTCInPoland
        {
            get 
            {
                if (_offsetUTCInPoland.Hours == 0) // offset not set
                    _offsetUTCInPoland = CalculateOffetUTCInPoland();
                return _offsetUTCInPoland;
            }
        }

        private static TimeSpan CalculateOffetUTCInPoland()
        {
            // contains start and end dates (in UTC)
            Tuple<DateTime,DateTime>[] daylightRanges = 
            {
                Tuple.Create<DateTime,DateTime>(new DateTime(2015, 03, 29, 1, 0, 0, DateTimeKind.Utc), new DateTime(2015, 10, 25, 2, 0, 0, DateTimeKind.Utc)),
                Tuple.Create<DateTime,DateTime>(new DateTime(2016, 03, 27, 1, 0, 0, DateTimeKind.Utc), new DateTime(2016, 10, 30, 2, 0, 0, DateTimeKind.Utc)),
                Tuple.Create<DateTime,DateTime>(new DateTime(2017, 03, 26, 1, 0, 0, DateTimeKind.Utc), new DateTime(2017, 10, 29, 2, 0, 0, DateTimeKind.Utc)),
                Tuple.Create<DateTime,DateTime>(new DateTime(2018, 03, 25, 1, 0, 0, DateTimeKind.Utc), new DateTime(2018, 10, 28, 2, 0, 0, DateTimeKind.Utc)),
                Tuple.Create<DateTime,DateTime>(new DateTime(2019, 03, 31, 1, 0, 0, DateTimeKind.Utc), new DateTime(2019, 10, 27, 2, 0, 0, DateTimeKind.Utc)),
            };

            var currentTime = DateTime.UtcNow;
            var tupleIndex = currentTime.Year - 2015;
            if(tupleIndex >= daylightRanges.Count())
                return new TimeSpan(1, 0, 0);

            var tuple = daylightRanges[tupleIndex];
            if (currentTime > tuple.Item1 && currentTime < tuple.Item2)
                return new TimeSpan(2, 0, 0);
            else
                return new TimeSpan(1, 0, 0);
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            WindowsAppInitializer.InitializeAsync();

            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
            GlobalCrashHandler.Configure();

#if WINDOWS_UWP
            IsMobile = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile";
#else
            IsMobile = true;
#endif

            var configuration = new LoggingConfiguration();
#if DEBUG
            configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new DebugTarget());
#endif
            configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new FileStreamingTarget() { RetainDays = 7 });
            configuration.IsEnabled = true;

            LogManagerFactory.DefaultConfiguration = configuration;

            Logger = LogManagerFactory.DefaultLogManager.GetLogger<App>();
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
                //this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;
            DispatcherHelper.Initialize();

            // expand window size 
            var applicationView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            applicationView.SetDesiredBoundsMode(Windows.UI.ViewManagement.ApplicationViewBoundsMode.UseCoreWindow);

            var locator = this.Resources["Locator"] as ViewModelLocator;
            NavService = locator.NavService;

            Window.Current.VisibilityChanged += (s, args) => Windows.Storage.ApplicationData.Current.LocalSettings.Values["AppRunning"] = args.Visible;

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
                        this.transitions.Add(c);
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                    await ResumeFromSuspension();

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
                        this.transitions.Add(c);
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                WykopSDK.WykopSDK.LocalStorage.InitAction();

                if(string.IsNullOrEmpty(e.Arguments))
                    NavService.NavigateTo("PivotPage");
                else
                    ProcessLaunchArguments(e.Arguments);
            }
            else
            {
                ProcessLaunchArguments(e.Arguments);
            }

            // Ensure the current window is active
            Window.Current.Activate();

#if WINDOWS_UWP
            if (IsMobile)
                StatusBarManager.Init();
            else
                StatusBarManager.Init(NavService.GetProgressBar(), NavService.GetProgressTextBlock());
#else
            StatusBarManager.Init();
#endif

            Messenger.Default.Send(new NotificationMessage("Init"));
        }

        private void ProcessLaunchArguments(string args)
        {
            if (string.IsNullOrEmpty(args)) return;

            if (IsMobile)
                NavService.InsertMainPage();
            else
                NavService.NavigateTo("PivotPage");

            var notification = JsonConvert.DeserializeObject<Notification>(args);
            if(notification.Type == NotificationType.EntryDirected || notification.Type == NotificationType.CommentDirected)
            {
                var VM = SimpleIoc.Default.GetInstance<NotificationsViewModel>(); // make sure it exists
                Messenger.Default.Send(new NotificationMessage<Notification>(notification, "Go to"));
            }
            else if(notification.Type == NotificationType.PM)
            {
                var VM = SimpleIoc.Default.GetInstance<MessagesViewModel>(); // make sure it exists
                Messenger.Default.Send(new NotificationMessage<string>(notification.AuthorName, "Go to"));
            }
            else
            {
                NavService.NavigateTo("AtNotificationsPage");
            }
        }

        protected override async void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            ShareTargetActivated = true;

            var data = args.ShareOperation.Data;
            var files = await data.GetStorageItemsAsync();
            var VM = SimpleIoc.Default.GetInstance<NewEntryViewModel>();
            VM.AddFiles(files);
            args.ShareOperation.ReportDataRetrieved();

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

            Window.Current.VisibilityChanged += (s, e) => Windows.Storage.ApplicationData.Current.LocalSettings.Values["AppRunning"] = e.Visible;

            NavService = SimpleIoc.Default.GetInstance<NavigationService>();

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
                        this.transitions.Add(c);
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

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
                        this.transitions.Add(c);
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                WykopSDK.WykopSDK.LocalStorage.InitAction();

                NavService.NavigateTo("NewEntryPage");
            }

            // Ensure the current window is active
            Window.Current.Activate();

            Messenger.Default.Send<NotificationMessage>(new NotificationMessage("Init"));
        }

#if WINDOWS_PHONE_APP
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
        protected override void OnActivated(IActivatedEventArgs e)
        {
            base.OnActivated(e);

            continuationManager = new ContinuationManager();

            Frame rootFrame = CreateRootFrame();

            if (rootFrame.Content == null)
                rootFrame.Navigate(typeof(HostPage));

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

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings.Values;

            if (currentPage != "DonationPage")
                localSettings["PageKey"] = currentPage;
            else
                localSettings.Remove("PageKey");

            localSettings.Remove("VM");

            if (currentFrame.DataContext != null &&
                currentFrame.DataContext is IResumable)
            {
                try
                {
                    var resumableVM = currentFrame.DataContext as IResumable;
                    await resumableVM.SaveState(currentPage);
                    localSettings["VM"] = resumableVM.GetName();
                } 
                catch(Exception ex)
                {
                    Logger.Error("Error saving state: ", ex);
                }
            }

            deferral.Complete();
        }

        private async Task ResumeFromSuspension()
        {
            Logger.Trace("Resuming from suspension.");

            var settings = Windows.Storage.ApplicationData.Current.LocalSettings.Values;
            if (!settings.ContainsKey("PageKey"))
            {
                NavService.NavigateTo("PivotPage");
                return;
            }

            var pageKey = (string)settings["PageKey"];

            if(!settings.ContainsKey("VM"))
            {
                NavService.InsertMainPage();
                NavService.NavigateTo(pageKey);
                return;
            }

            var viewModelName = (string)settings["VM"];

            bool resumed = false;
            IResumable viewModel = null;
            if (viewModelName == "MainViewModel")
                viewModel = SimpleIoc.Default.GetInstance<MainViewModel>();
            else if (viewModelName == "ProfilesViewModel")
                viewModel = SimpleIoc.Default.GetInstance<ProfilesViewModel>();
            else if (viewModelName == "NewEntryViewModel")
                viewModel = SimpleIoc.Default.GetInstance<NewEntryViewModel>();
            else if(viewModelName == "MessagesViewModel")
                viewModel = SimpleIoc.Default.GetInstance<MessagesViewModel>();

            try
            {
                resumed = await viewModel.LoadState(pageKey);
            } 
            catch(Exception e)
            {
                Logger.Error("Error loading state: ", e);
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