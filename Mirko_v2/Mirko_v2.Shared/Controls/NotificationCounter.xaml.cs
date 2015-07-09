using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Controls
{
    public sealed partial class NotificationCounter : UserControl
    {
        private int targetValue;
        private int currentValue;
        private const int threshold = 5;
        private const int flipsMax = 5; // number of flips in fast animation
        private int flipsCounter = 0;

        private static Random RandomGenerator = new Random();

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

        private static void CountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = d as NotificationCounter;
            var count = (uint?)e.NewValue;
            if (count != null)
            {
                c.targetValue = (int)count.Value;

                if (Math.Abs(c.currentValue - c.targetValue) < threshold)
                    c.SlowFlip_1.Begin();
                else
                    c.FastFlip_1.Begin();

                c.Foreground = c.targetValue > 0 ? c.NotificationsBrush : c.NoNotificationsBrush;
            }
        }

        
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
            if(!@override)
                c.Foreground = c.Count > 0 ? c.NotificationsBrush : c.NoNotificationsBrush;
        }        

        public NotificationCounter()
        {
            this.InitializeComponent();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.GetInstance<Mirko_v2.ViewModel.SettingsViewModel>().ThemeChanged += NotificationCounter_ThemeChanged;
        }

        private void NotificationCounter_ThemeChanged(object sender, EventArgs e)
        {
            Foreground = Count > 0 ? NotificationsBrush : NoNotificationsBrush;
        }

        private void SlowFlip_1_Completed(object sender, object e)
        {
            bool doMoreFlips = true;

            if (currentValue < targetValue)
                currentValue++;
            else if (currentValue > targetValue)
                currentValue--;
            else
                doMoreFlips = false;

            NumberTB.Text = currentValue > 0 ? currentValue.ToString() : "";
            if(doMoreFlips)
                SlowFlip_2.Begin();
        }

        private void SlowFlip_2_Completed(object sender, object e)
        {
            if (currentValue != targetValue)
                SlowFlip_1.Begin();
        }

        private void FastFlip_1_Completed(object sender, object e)
        {
            int diff = Math.Abs(currentValue - targetValue);
            bool doMoreFlips = true;
            int random = 1;

            if (diff > 1)
                random = RandomGenerator.Next(1, diff / 2);

            if (currentValue < targetValue)
                currentValue += random;
            else if (currentValue > targetValue)
                currentValue -= random;
            else
                doMoreFlips = false;

            NumberTB.Text = currentValue > 0 ? currentValue.ToString() : "";

            if(doMoreFlips)
                FastFlip_2.Begin();
        }

        private void FastFlip_2_Completed(object sender, object e)
        {
            flipsCounter++;
            if (flipsCounter <= flipsMax)
            {
                FastFlip_1.Begin();
            }
            else
            {
                SlowFlip_1.Begin();

                flipsCounter = 0;
            }
        }
    }
}
