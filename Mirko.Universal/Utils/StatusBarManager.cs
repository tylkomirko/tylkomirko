using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Mirko.ViewModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Mirko.Utils
{
    public static class StatusBarManager
    {
        private static DispatcherTimer Timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 7) };
        private static bool isMobile = false;

        private static ProgressBar ProgressBar { get; set; }
        private static TextBlock ProgressText { get; set; }

        private static void Timer_Tick(object sender, object e)
        {
            if (isMobile)
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.Text = " ";
            }
            else
            {
                ProgressText.Text = " ";
            }

            Timer.Tick -= Timer_Tick;
        }

        private static Action<StatusBarProgressIndicator> ShowProgressIndicator = new Action<StatusBarProgressIndicator>(async prog =>
        {
            await DispatcherHelper.RunAsync(async () => await prog.ShowAsync());
        });

        private static Action<StatusBarProgressIndicator> HideProgressIndicator = new Action<StatusBarProgressIndicator>(async prog =>
        {
            await prog.HideAsync();
        });

        private static Action<StatusBar> ShowSB = new Action<StatusBar>(async sb =>
        {
            await sb.ShowAsync();
        });

        private static Action<StatusBar> HideSB = new Action<StatusBar>(async sb =>
        {
            await sb.HideAsync();
        });

        public static void Paint(ElementTheme theme)
        {
            if (isMobile)
            {
                var statusBar = StatusBar.GetForCurrentView();

                if (theme == ElementTheme.Light)
                {
                    statusBar.BackgroundColor = Colors.White;
                    statusBar.ForegroundColor = (Color)Application.Current.Resources["StatusBarForegroundLight"];
                }
                else
                {
                    statusBar.BackgroundColor = Colors.Black;
                    statusBar.ForegroundColor = Colors.White;
                }
            }
            else
            {
                if(theme == ElementTheme.Light)
                {
                    ProgressBar.Background = new SolidColorBrush(Colors.White);
                    ProgressBar.Foreground = new SolidColorBrush((Color)Application.Current.Resources["StatusBarForegroundLight"]);
                    ProgressText.Foreground = new SolidColorBrush((Color)Application.Current.Resources["StatusBarForegroundLight"]);
                }
                else
                {
                    ProgressBar.Background = new SolidColorBrush(Colors.Black);
                    ProgressBar.Foreground = new SolidColorBrush(Colors.White);
                    ProgressText.Foreground = new SolidColorBrush(Colors.White);
                }
            }
        }

        public static void Init(ProgressBar bar = null, TextBlock tb = null)
        {
            isMobile = AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile";

            var settingsVM = SimpleIoc.Default.GetInstance<SettingsViewModel>();
            settingsVM.ThemeChanged += (s, args) => Paint((args as ThemeChangedEventArgs).Theme);

            if (isMobile)
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundOpacity = (double)Application.Current.Resources["AppHeaderOpacity"];
                statusBar.ProgressIndicator.Text = " ";
                statusBar.ProgressIndicator.ProgressValue = 0.0;
                ShowProgressIndicator(statusBar.ProgressIndicator);
            }
            else
            {
                ProgressBar = bar;
                ProgressText = tb;

                ProgressBar.Value = 0.0;
                ProgressText.Text = " ";
            }

            Paint(settingsVM.SelectedTheme);
        }

        public static async Task ShowTextAsync(string txt)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                if (isMobile)
                {
                    var statusBar = StatusBar.GetForCurrentView();

                    statusBar.ProgressIndicator.Text = txt;
                    statusBar.ProgressIndicator.ProgressValue = 0.0;
                }
                else
                {
                    ProgressBar.Value = 0.0;
                    ProgressText.Text = txt;
                }

                if (Timer.IsEnabled)
                    Timer.Stop();

                Timer.Start();
                Timer.Tick += Timer_Tick;

            });            
        }

        public static void ShowText(string txt)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(async () => await ShowTextAsync(txt));
        }

        public static async Task<string> GetText()
        {
            string text = null;

            await DispatcherHelper.RunAsync(() =>
            {
                var statusBar = StatusBar.GetForCurrentView();
                text = statusBar.ProgressIndicator.Text;
            });

            return text;
        }

        public static async Task ShowTextAndProgressAsync(string txt)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                if (isMobile)
                {
                    var statusBar = StatusBar.GetForCurrentView();

                    statusBar.ProgressIndicator.Text = txt;
                    statusBar.ProgressIndicator.ProgressValue = null;
                }
                else
                {
                    ProgressBar.IsIndeterminate = true;
                    ProgressText.Text = txt;
                }
            });
        }

        public static void ShowTextAndProgress(string txt)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(async () => await ShowTextAndProgressAsync(txt));
        }

        public static async Task HideStatusBarAsync()
        {
            await DispatcherHelper.RunAsync(async () => 
            {
                if (isMobile)
                {
                    var statusBar = StatusBar.GetForCurrentView();
                    await statusBar.HideAsync();
                }
                else
                {
                    ProgressBar.Visibility = Visibility.Collapsed;
                    ProgressText.Visibility = Visibility.Collapsed;
                }
            });
        }

        public static void HideStatusBar()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(async () => await HideStatusBarAsync());
        }

        public static async Task ShowStatusBarAsync()
        {
            await DispatcherHelper.RunAsync(async () =>
            {
                if (isMobile)
                {
                    var statusBar = StatusBar.GetForCurrentView();
                    await statusBar.ShowAsync();
                }
                else
                {
                    ProgressBar.Visibility = Visibility.Visible;
                    ProgressText.Visibility = Visibility.Visible;
                }
            });
        }

        public static void ShowStatusBar()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(async () => await ShowStatusBarAsync());
        }

        public static async Task ShowProgressAsync(double? prog = null)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                if (isMobile)
                {
                    var statusBar = StatusBar.GetForCurrentView();
                    statusBar.ProgressIndicator.Text = " ";
                    statusBar.ProgressIndicator.ProgressValue = prog;
                }
            });
        }

        public static void ShowProgress()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(async () => await ShowProgressAsync());
        }

        public static async Task HideProgressAsync()
        {
            await DispatcherHelper.RunAsync(() =>
            {
                if (isMobile)
                {
                    var statusBar = StatusBar.GetForCurrentView();
                    statusBar.ProgressIndicator.ProgressValue = 0.0;
                    statusBar.ProgressIndicator.Text = " ";
                }
                else
                {
                    ProgressBar.IsIndeterminate = false;
                    ProgressBar.Value = 0.0;
                    ProgressText.Text = " ";
                }
            });
        }

        public static void HideProgress()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(async () => await HideProgressAsync());
        }
    }
}
