using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Mirko_v2.Controls;
using Mirko_v2.Pages;
using Mirko_v2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Mirko_v2.ViewModel
{
    public class NavigationService : INavigationService, IDisposable
    {
        struct CachedPage
        {
            public UserControl Page;
            public CommandBar AppBar;
        };

        private const int cachedPagesCount = 4;
        private bool navigatedToRootPage = false;
        private Page rootPage = null;
        private Frame rootPageFrame = null;
        private AppHeader rootPageHeader = null;
        private Popup rootPagePopup = null;

        private readonly StackList<Type> backStack = new StackList<Type>();
        private readonly Dictionary<string, Type> pagesNames = new Dictionary<string, Type>();
        private readonly Dictionary<Type, CachedPage> pagesCache = new Dictionary<Type, CachedPage>();
        private readonly List<string> framesWithoutHeader = new List<string>() { "EmbedPage", "SettingsPage", "NewEntryPage" };

        public delegate void NavigatingEventHandler(object source, StringEventArgs newPage);
        public event NavigatingEventHandler Navigating;

#if WINDOWS_PHONE_APP
        public NavigationService()
        {
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (CanGoBack())
            {
                if (CurrentPageKey == "HashtagFlipPage")
                {
                    var VM = SimpleIoc.Default.GetInstance<NotificationsViewModel>();
                    if (VM.CurrentHashtagNotifications.Count == 0)
                    {
                        var entry = backStack.First(typeof(HashtagNotificationsPage));
                        if(entry != null)
                            backStack.Remove(entry);
                    }
                }

                GoBack();
                e.Handled = true;
            }
        }
#endif

        public object CurrentData { get; set; }

        private CachedPage GetCachedPage(Type type)
        {
            var cachedPage = new CachedPage();

            UserControl content = null;
            CommandBar appBar = null;

            if (!pagesCache.ContainsKey(type))
            {
                if (pagesCache.Count > cachedPagesCount)
                {
                    var item = pagesCache.First(x => x.Key.Name != "PivotPage");
                    var page = item.Value;
                    var key = item.Key;

                    var dispose = page.Page as IDisposable;
                    if (dispose != null)
                        dispose.Dispose();

                    page.Page = null;
                    page.AppBar = null;

                    pagesCache.Remove(key);
                }

                content = (UserControl)Activator.CreateInstance(type);
                var hasAppBar = content as IHaveAppBar;
                if (hasAppBar != null)
                    appBar = hasAppBar.CreateCommandBar();

                cachedPage.Page = content;
                cachedPage.AppBar = appBar;

                pagesCache.Add(type, cachedPage);
            }
            else
            {
                cachedPage = pagesCache[type];
            }

            return cachedPage;
        }

        public void RegisterPage(string key, Type type)
        {
            pagesNames.Add(key, type);
        }

        public void NavigateTo(string key)
        {
            if (string.IsNullOrEmpty(key) || CurrentPageKey == key)
                return;

            if(key != "PivotPage") // PivotPage handles it on it's own
                App.TelemetryClient.TrackPageView(key);

            var currentFrame = Window.Current.Content as Frame;

            if(!navigatedToRootPage)
            {
                currentFrame.Navigate(typeof(HostPage));
                rootPage = currentFrame.Content as HostPage;
                rootPageFrame = rootPage.FindName("MainFrame") as Frame;
                rootPageHeader = rootPage.FindName("AppHeader") as AppHeader;
                rootPagePopup = rootPage.FindName("SuggestionsPopup") as Popup;

                navigatedToRootPage = true;
            }

            var type = pagesNames[key];
            var page = GetCachedPage(type);

            rootPageFrame.Content = page.Page;
            rootPage.BottomAppBar = page.AppBar;          

            if (framesWithoutHeader.Contains(key))
                rootPageHeader.Visibility = Visibility.Collapsed;
            else
                rootPageHeader.Visibility = Visibility.Visible;

            if(key != "LoginPage")
                backStack.Push(type);

            CurrentPageKey = key;

            if (Navigating != null)
                Navigating(this, new StringEventArgs(key));
        }

        public void NavigateTo(string key, object data)
        {
            var currentFrame = Window.Current.Content as Frame;
            var nextFrameType = pagesNames[key];
            if (currentFrame != null && (currentFrame.CurrentSourcePageType == null || currentFrame.CurrentSourcePageType != nextFrameType))
            {
                CurrentData = data;
                CurrentPageKey = key;

                currentFrame.Navigate(nextFrameType, data);
            }
        }

        public bool CanGoBack()
        {
            var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();
            if (!mainVM.CanGoBack)
                return false;

            if (backStack.Count() <= 1)
            {
                Application.Current.Exit(); 
                return false;
            }

            return true;
        }

        public void GoBack()
        {
            if (!CanGoBack()) return;

            backStack.Pop();
            var type = backStack.Peek();
            var page = GetCachedPage(type);

            rootPageFrame.Content = page.Page;
            rootPage.BottomAppBar = page.AppBar;

            CurrentPageKey = pagesNames.Single(x => x.Value == type).Key;

            if (framesWithoutHeader.Contains(CurrentPageKey))
                rootPageHeader.Visibility = Visibility.Collapsed;
            else
                rootPageHeader.Visibility = Visibility.Visible;

            if (Navigating != null)
                Navigating(this, new StringEventArgs(CurrentPageKey));
        }

        public void Dispose()
        {
#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
#endif
        }

        private string _currentPageKey = null;
        public string CurrentPageKey
        {
            get { return _currentPageKey; }
            set { _currentPageKey = value; }
        }

        public UserControl GetFrame(string pageKey)
        {
            var type = pagesNames[pageKey];
            if (pagesCache.ContainsKey(type))
                return pagesCache[type].Page;
            else
                return null;
        }

        public void InsertMainPage()
        {
            backStack.Push(typeof(PivotPage));
        }

        public UserControl CurrentFrame()
        {
            return rootPageFrame.Content as UserControl;
        }

        public AppHeader GetAppHeader()
        {
            return rootPageHeader;
        }

        public Popup GetPopup()
        {
            return rootPagePopup;
        }
    }
}
