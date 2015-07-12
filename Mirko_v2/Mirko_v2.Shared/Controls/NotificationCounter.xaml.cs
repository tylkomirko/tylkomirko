using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Mirko_v2.Utils;
using Mirko_v2.ViewModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Controls
{
    public sealed partial class NotificationCounter : UserControl
    {
        private int TargetValue;
        private int CurrentValue;
        private const int Threshold = 5; // threshold between slow and fast flip
        private CancellationTokenSource CTS = null;
        private Task LastTask = null;

        private static Random RandomGenerator = new Random();

        #region Colour props
        public SolidColorBrush NoNotificationsBrush
        {
            get { return (SolidColorBrush)GetValue(NoNotificationsBrushProperty); }
            set { SetValue(NoNotificationsBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NoNotificationsBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoNotificationsBrushProperty =
            DependencyProperty.Register("NoNotificationsBrush", typeof(SolidColorBrush), typeof(NotificationCounter), new PropertyMetadata(null));

        public SolidColorBrush NotificationsBrush
        {
            get { return (SolidColorBrush)GetValue(NotificationsBrushProperty); }
            set { SetValue(NotificationsBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NotificationsBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotificationsBrushProperty =
            DependencyProperty.Register("NotificationsBrush", typeof(SolidColorBrush), typeof(NotificationCounter), new PropertyMetadata(null));

        new public SolidColorBrush Foreground
        {
            get { return (SolidColorBrush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Foreground.  This enables animation, styling, binding, etc...
        new public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(SolidColorBrush), typeof(NotificationCounter), new PropertyMetadata(null, ForegroundChanged));

        private static void ForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = d as NotificationCounter;
            var b = e.NewValue as SolidColorBrush;

            c.PrefixTB.Foreground = b;
            c.NumberTB.Foreground = b;
        }

        public bool OverrideForeground
        {
            get { return (bool)GetValue(OverrideForegroundProperty); }
            set { SetValue(OverrideForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OverrideForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OverrideForegroundProperty =
            DependencyProperty.Register("OverrideForeground", typeof(bool), typeof(NotificationCounter), new PropertyMetadata(false, OverrideForegroundChanged));

        private static void OverrideForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = d as NotificationCounter;

            bool @override = (bool)e.NewValue;
            if (!@override)
                c.Foreground = c.Count > 0 ? c.NotificationsBrush : c.NoNotificationsBrush;
        }
        #endregion

        public string Prefix
        {
            get { return (string)GetValue(PrefixProperty); }
            set { SetValue(PrefixProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Prefix.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrefixProperty =
            DependencyProperty.Register("Prefix", typeof(string), typeof(NotificationCounter), new PropertyMetadata(""));

        public uint Count
        {
            get { return (uint)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Count.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register("Count", typeof(uint), typeof(NotificationCounter), new PropertyMetadata(0, CountChanged));

        private static void CountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = d as NotificationCounter;

            if (c.LastTask != null)
            {
                c.CTS.Cancel();
                c.CTS.Dispose();
                c.CTS = null;
            }

            c.TargetValue = (int)((uint)e.NewValue);

            var cts = new CancellationTokenSource();
            var task = DispatcherHelper.RunAsync(async () => await c.AnimateCountChange(cts.Token)).AsTask(cts.Token);
            c.LastTask = task;
            c.CTS = cts;
        }

        private async Task AnimateCountChange(CancellationToken token)
        {
            if (!OverrideForeground)
                Foreground = TargetValue > 0 ? NotificationsBrush : NoNotificationsBrush;

            if (CurrentValue == 0 && TargetValue == 0)
                return;

            if (token.IsCancellationRequested)
                return;

            int diff = Math.Abs(TargetValue - CurrentValue);
            bool countUp = TargetValue > CurrentValue;
            bool useFastAnimation = diff > Threshold ? true : false;

            if (useFastAnimation)
            {
                while(true)
                {
                    if (token.IsCancellationRequested)
                        return;

                    int currValue = CurrentValue;
                    int maxValue = countUp ? TargetValue - 2 : CurrentValue - 2;
                    int randomMax = maxValue - currValue;

                    if (randomMax <= 1)
                        break;

                    int random = RandomGenerator.Next(1, randomMax);
                    if (random > randomMax)
                        random = randomMax;

                    await FastFlip_1.BeginAsync();

                    CurrentValue += countUp ? random : -random;
                    NumberTB.Text = CurrentValue > 0 ? CurrentValue.ToString() : "";

                    await FastFlip_2.BeginAsync();
                }

                countUp = TargetValue > CurrentValue;
            }

            while(CurrentValue != TargetValue)
            {
                if (token.IsCancellationRequested)
                    return;

                await SlowFlip_1.BeginAsync();

                CurrentValue += countUp ? 1 : -1;
                NumberTB.Text = CurrentValue > 0 ? CurrentValue.ToString() : "";

                await SlowFlip_2.BeginAsync();
            }
        }

        public NotificationCounter()
        {
            this.InitializeComponent();
            SimpleIoc.Default.GetInstance<SettingsViewModel>().ThemeChanged += (s, e) => Foreground = Count > 0 ? NotificationsBrush : NoNotificationsBrush;
        }
    }
}
