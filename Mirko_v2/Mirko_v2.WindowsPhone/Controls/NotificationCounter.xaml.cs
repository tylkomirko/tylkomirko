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

        private static void CountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = d as NotificationCounter;
            var count = (uint?)e.NewValue;
            if (count != null)
            {
                c.targetValue = (int)count.Value;
                if (c.targetValue == 0) return;

                if (Math.Abs(c.currentValue - c.targetValue) < threshold)
                    c.SlowFlip_1.Begin();
                else
                    c.FastFlip_1.Begin();
            }
        }

        new public SolidColorBrush Foreground
        {
            get { return (SolidColorBrush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Foreground.  This enables animation, styling, binding, etc...
        new public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(SolidColorBrush), typeof(NotificationCounter), new PropertyMetadata(null));

        public NotificationCounter()
        {
            this.InitializeComponent();
        }

        private void SlowFlip_1_Completed(object sender, object e)
        {
            if (currentValue < targetValue)
                currentValue++;
            else if (currentValue > targetValue)
                currentValue--;
            else
                return;

            NumberTB.Text = currentValue.ToString();
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
            int random = 1;

            if (diff > 1)
                random = RandomGenerator.Next(1, diff / 2);

            if (currentValue < targetValue)
                currentValue += random;
            else if (currentValue > targetValue)
                currentValue -= random;
            else
                return;

            NumberTB.Text = currentValue.ToString();
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
