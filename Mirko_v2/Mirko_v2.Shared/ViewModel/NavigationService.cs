using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Mirko_v2.Controls;
using Mirko_v2.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Mirko_v2.ViewModel
{
    public class NavigationService : INavigationService, IDisposable
    {
#if WINDOWS_PHONE_APP
        public NavigationService()
        {
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            _framesWithoutHeader = new List<string>() { "EmbedPage" };
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
                        var currentFrame = Window.Current.Content as Frame;
                        var backStack = currentFrame.BackStack;

                        var entry = backStack.FirstOrDefault(x => x.SourcePageType == typeof(HashtagNotificationsPage));
                        if(entry != null)
                            backStack.Remove(entry);
                    }
                }

                GoBack();
                e.Handled = true;
            }
        }
#endif

        private bool NavigatedToRootPage = false;
        private Page _rootPage = null;
        private Frame _rootPageFrame = null;
        private AppHeader _rootPageHeader = null;
        private AppBar _rootPageAppBar = null;

        private readonly Stack<Type> _stack = new Stack<Type>();
        private readonly Dictionary<string, Type> _framesNames = new Dictionary<string, Type>();
        private readonly Dictionary<Type, UserControl> _framesContent = new Dictionary<Type, UserControl>();
        private readonly List<string> _framesWithoutHeader = null;
        public object CurrentData { get; set; }

        public void RegisterPage(string key, Type type)
        {
            _framesNames.Add(key, type);
        }

        public void NavigateTo(string key)
        {
            var currentFrame = Window.Current.Content as Frame;

            if(!NavigatedToRootPage)
            {
                currentFrame.Navigate(typeof(HostPage));
                _rootPage = currentFrame.Content as HostPage;
                _rootPageFrame = _rootPage.FindName("MainFrame") as Frame;
                _rootPageHeader = _rootPage.FindName("AppHeader") as AppHeader;
                _rootPageAppBar = _rootPage.BottomAppBar;

                NavigatedToRootPage = true;
            }

            var type = _framesNames[key];
            UserControl content = null;

            if(!_framesContent.ContainsKey(type))
            {
                content = (UserControl)Activator.CreateInstance(type);
                _framesContent.Add(type, content);
            }

            content = _framesContent[type];
            _rootPageFrame.Content = content;
            //var appBarTemplate = content.Resources["AppBar"] as DataTemplate;
            //_rootPageAppBar.Content = appBarTemplate.LoadContent();

            /*
            if(content is IHaveAppBar)
            {
                var commandBar = (content as IHaveAppBar).CreateCommandBar();
                _rootPageAppBar = commandBar;
            }*/

            if (_framesWithoutHeader.Contains(key))
                _rootPageHeader.Visibility = Visibility.Collapsed;
            else
                _rootPageHeader.Visibility = Visibility.Visible;

            _stack.Push(type);
            CurrentPageKey = key;

            /*var nextFrameType = _framesDictionary[key];
            if (currentFrame != null && (currentFrame.CurrentSourcePageType == null || currentFrame.CurrentSourcePageType != nextFrameType))
            {
                CurrentData = null;
                CurrentPageKey = key;

                currentFrame.Navigate(nextFrameType);
            }*/
        }

        public void NavigateTo(string key, object data)
        {
            var currentFrame = Window.Current.Content as Frame;
            var nextFrameType = _framesNames[key];
            if (currentFrame != null && (currentFrame.CurrentSourcePageType == null || currentFrame.CurrentSourcePageType != nextFrameType))
            {
                CurrentData = data;
                CurrentPageKey = key;

                currentFrame.Navigate(nextFrameType, data);
            }
        }

        public bool CanGoBack()
        {
            /*
            var currentFrame = Window.Current.Content as Frame;
            return currentFrame != null && currentFrame.CanGoBack;*/

            if (_stack.Count <= 1)
            {
                Application.Current.Exit(); 
                return false;
            }

            return true;
        }

        public void GoBack()
        {
            if (!CanGoBack()) return;

            _stack.Pop();
            var type = _stack.Peek();
            UserControl content = null;

            if (!_framesContent.ContainsKey(type))
            {
                content = (UserControl)Activator.CreateInstance(type);
                _framesContent.Add(type, content);
            }

            content = _framesContent[type];
            _rootPageFrame.Content = content;

            CurrentPageKey = _framesNames.Single(x => x.Value == type).Key;

            if (_framesWithoutHeader.Contains(CurrentPageKey))
                _rootPageHeader.Visibility = Visibility.Collapsed;
            else
                _rootPageHeader.Visibility = Visibility.Visible;

            /*
            if (!CanGoBack()) return;
            var frame = Window.Current.Content as Frame;
            if (frame != null)
            {
                frame.GoBack();

                CurrentPageKey = _framesNames.Single(x => x.Value == frame.CurrentSourcePageType).Key;
            }*/
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
            var type = _framesNames[pageKey];
            if (_framesContent.ContainsKey(type))
                return _framesContent[type];
            else
                return null;
        }

        public void InsertMainPage()
        {
            _stack.Push(typeof(PivotPage));
        }

        public UserControl CurrentFrame()
        {
            return _rootPageFrame.Content as UserControl;
        }
    }
}
