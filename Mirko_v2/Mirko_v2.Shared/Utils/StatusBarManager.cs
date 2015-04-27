using System;
using System.Threading;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

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
            await prog.ShowAsync();
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

            statusBar.BackgroundOpacity = 0.9;
            statusBar.ProgressIndicator.Text = " ";
            statusBar.ProgressIndicator.ProgressValue = 0.0;
            ShowProgressIndicator(statusBar.ProgressIndicator);
        }

        public static void ShowText(string txt)
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
        }

        public static string GetText()
        {
            var statusBar = StatusBar.GetForCurrentView();
            return statusBar.ProgressIndicator.Text;
        }

        public static void ShowTextAndProgress(string txt)
        {
            var statusBar = StatusBar.GetForCurrentView();

            statusBar.BackgroundOpacity = 0.9;
            statusBar.ProgressIndicator.Text = txt;
            statusBar.ProgressIndicator.ProgressValue = null;
        }

        public static void HideStatusBar()
        {
            var statusBar = StatusBar.GetForCurrentView();
            HideSB(statusBar);
        }

        public static void ShowStatusBar()
        {
            var statusBar = StatusBar.GetForCurrentView();
            ShowSB(statusBar);
        }

        public static void ShowProgress()
        {
            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = " ";
            statusBar.ProgressIndicator.ProgressValue = null;
        }

        public static void HideProgress()
        {
            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.ProgressValue = 0.0;
            statusBar.ProgressIndicator.Text = " ";
        }
    }
}
