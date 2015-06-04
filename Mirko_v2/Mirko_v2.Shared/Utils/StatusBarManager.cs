using GalaSoft.MvvmLight.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Mirko_v2.Utils
{
    public static class StatusBarManager
    {
        private static DispatcherTimer Timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 8) };

        private static void Timer_Tick(object sender, object e)
        {
            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = " ";

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

        public static void Init()
        {
            var statusBar = StatusBar.GetForCurrentView();

            if (App.Current.RequestedTheme == ApplicationTheme.Light)
            {
                statusBar.BackgroundColor = Colors.White;
                statusBar.ForegroundColor = (Application.Current.Resources["LogoFill"] as SolidColorBrush).Color;
            }

            statusBar.BackgroundOpacity = 0.9;
            statusBar.ProgressIndicator.Text = " ";
            statusBar.ProgressIndicator.ProgressValue = 0.0;
            ShowProgressIndicator(statusBar.ProgressIndicator);
        }

        public static async Task ShowText(string txt)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                var statusBar = StatusBar.GetForCurrentView();

                statusBar.ProgressIndicator.Text = txt;
                statusBar.ProgressIndicator.ProgressValue = 0.0;

                if (Timer.IsEnabled)
                {
                    Timer.Stop();
                    Timer.Start();

                    Timer.Tick += Timer_Tick;
                }
                else
                {
                    Timer.Start();
                    Timer.Tick += Timer_Tick;
                }
            });            
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

        public static async Task ShowTextAndProgress(string txt)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                var statusBar = StatusBar.GetForCurrentView();
                
                statusBar.BackgroundOpacity = 0.9;
                statusBar.ProgressIndicator.Text = txt;
                statusBar.ProgressIndicator.ProgressValue = null;
            });
        }

        public static async Task HideStatusBar()
        {
            await DispatcherHelper.RunAsync(async () => 
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
            });
        }

        public static async Task ShowStatusBar()
        {
            await DispatcherHelper.RunAsync(async () =>
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.ShowAsync();
            });
        }

        public static async Task ShowProgress()
        {
            await DispatcherHelper.RunAsync(() =>
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.Text = " ";
                statusBar.ProgressIndicator.ProgressValue = null;
            });
        }

        public static async Task HideProgress()
        {
            await DispatcherHelper.RunAsync(() =>
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.ProgressValue = 0.0;
                statusBar.ProgressIndicator.Text = " ";
            });
        }
    }
}
