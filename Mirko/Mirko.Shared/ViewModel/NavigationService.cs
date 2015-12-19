using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Mirko.Controls;
using Mirko.Pages;
using Mirko.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Mirko.ViewModel
{
    public class NavigationService : INavigationService, IDisposable
    {
        private const int cachedPagesCount = 4;
        private bool navigatedToRootPage = false;
        private Page rootPage = null;
        private Page rootPageFrame = null;
        private AppHeader rootPageHeader = null;
        private Popup rootPagePopup = null;
        private bool navigatedToPivotPage = false;

        private readonly StackList<Type> backStack = new StackList<Type>();
        private readonly Dictionary<string, Type> pagesNames = new Dictionary<string, Type>();
        private readonly Dictionary<Type, Page> pagesCache = new Dictionary<Type, Page>();
        private readonly List<string> framesWithoutHeader = new List<string>() { "EmbedPage", "SettingsPage", "NewEntryPage", "BlacklistPage" };

        public delegate void NavigatingEventHandler(object source, StringEventArgs newPage);
        public event NavigatingEventHandler Navigating;

        public NavigationService()
        {
#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#else
            if(Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#endif
        }

        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            var handled = e.Handled;
            e.Handled = true;
            BackPressed();
        }

        public void BackPressed()
        {
            if (CanGoBack())
            {
                if (CurrentPageKey == "HashtagFlipPage")
                {
                    var VM = SimpleIoc.Default.GetInstance<NotificationsViewModel>();
                    if (VM.CurrentHashtagNotifications.Count == 0)
                    {                        
                        var entry = backStack.First(typeof(HashtagNotificationsPage));
                        if (entry != null)
                            backStack.Remove(entry);
                    }
                }

                GoBack();
            }
        }

        public object CurrentData { get; set; }

        private Page GetCachedPage(Type type)
        {
            Page cachedPage = null;

            if (!pagesCache.ContainsKey(type))
            {
                if (pagesCache.Count > cachedPagesCount)
                {
                    var item = pagesCache.First(x => x.Key.Name != "PivotPage");
                    var page = item.Value;
                    var key = item.Key;

                    var dispose = page as IDisposable;
                    if (dispose != null)
                        dispose.Dispose();

                    pagesCache.Remove(key);
                }

                cachedPage = (Page)Activator.CreateInstance(type);
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
                if (App.IsMobile)
                {
                    currentFrame.Navigate(typeof(HostPage));
                    rootPage = currentFrame.Content as HostPage;
                }
#if WINDOWS_UWP
                else
                {
                    currentFrame.Navigate(typeof(DualHostPage));
                    rootPage = currentFrame.Content as DualHostPage;
                }
#endif

                rootPageFrame = rootPage.FindName("MainFrame") as Page;
                rootPageHeader = rootPage.FindName("AppHeader") as AppHeader;
                rootPagePopup = rootPage.FindName("SuggestionsPopup") as Popup;

                navigatedToRootPage = true;
            }

            var type = pagesNames[key];
            var page = GetCachedPage(type);

            if (!App.IsMobile && !navigatedToPivotPage)
            {
                var frame = rootPage.FindName("FirstFrame") as Page;
                frame.Content = GetCachedPage(pagesNames["PivotPage"]);

                string secondPageName = (key == "PivotPage") ? "EmptyPage" : key;
                var secondPageType = pagesNames[secondPageName];
                rootPageFrame.Content = GetCachedPage(secondPageType);

                if (secondPageName != "EmptyPage")
                    backStack.Push(pagesNames["EmptyPage"]);

                backStack.Push(secondPageType);

                navigatedToPivotPage = true;
            }
            else if (!App.IsMobile && key == "EmbedPage")
            {
                currentFrame.Content = page;
                backStack.Push(type);
            }
            else
            {
                rootPageFrame.Content = page;

                if (key != "LoginPage")
                    backStack.Push(type);
            }

            if (App.IsMobile && framesWithoutHeader.Contains(key))
                rootPageHeader.Visibility = Visibility.Collapsed;
            else
                rootPageHeader.Visibility = Visibility.Visible;

            CurrentPageKey = key;

#if WINDOWS_UWP
            if(backStack.Count() > 1)
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
#endif

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
            Type poppedType = null;

            if(App.IsMobile)
            {
                poppedType = backStack.Pop();
            }
            else
            {
                if (backStack.Count() > 1)
                    poppedType = backStack.Pop();
            }

            var type = backStack.Peek();
            var page = GetCachedPage(type);

            string previousKey = null;
            if(poppedType != null)
                previousKey = pagesNames.Single(x => x.Value == poppedType).Key;

            CurrentPageKey = pagesNames.Single(x => x.Value == type).Key;

#if WINDOWS_UWP
            if(!App.IsMobile && !string.IsNullOrEmpty(previousKey) && previousKey == "EmbedPage")
            {
                var currentFrame = Window.Current.Content as Frame;
                currentFrame.Navigate(typeof(DualHostPage));
            }
#endif

            rootPageFrame.Content = page;

            if (App.IsMobile && framesWithoutHeader.Contains(CurrentPageKey))
                rootPageHeader.Visibility = Visibility.Collapsed;
            else
                rootPageHeader.Visibility = Visibility.Visible;

#if WINDOWS_UWP
            if (backStack.Count() > 1)
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            else
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
#endif

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
                return pagesCache[type];
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

#if WINDOWS_UWP
        public ProgressBar GetProgressBar()
        {
            var currentFrame = Window.Current.Content as Frame;
            var currentPage = currentFrame.Content as DualHostPage;
            return currentPage.GetDescendant<ProgressBar>("ProgressBar");
        }

        public TextBlock GetProgressTextBlock()
        {
            var currentFrame = Window.Current.Content as Frame;
            var currentPage = currentFrame.Content as DualHostPage;
            return currentPage.GetDescendant<TextBlock>("ProgressText");
        }
#endif
    }
}
