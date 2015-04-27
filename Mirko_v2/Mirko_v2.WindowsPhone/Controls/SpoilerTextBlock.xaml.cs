using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Controls
{
    public sealed partial class SpoilerTextBlock : UserControl
    {
        public SpoilerTextBlock()
        {
            this.InitializeComponent();

            isTextHidden = true;
        }

        private bool isTextHidden { get; set; }



        public List<Inline> HiddenContent
        {
            get { return (List<Inline>)GetValue(HiddenContentProperty); }
            set { SetValue(HiddenContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HiddenSpan.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HiddenContentProperty =
            DependencyProperty.Register("HiddenContent", typeof(List<Inline>), typeof(SpoilerTextBlock), null);
      


        public string HiddenText { get; set; }

        public static string GetHiddenText(DependencyObject obj)
        {
            return (string)obj.GetValue(HiddenTextProperty);
        }

        public static void SetHiddenText(DependencyObject obj, string value)
        {
            obj.SetValue(HiddenTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for HiddenText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HiddenTextProperty =
            DependencyProperty.Register("HiddenText", typeof(string), typeof(SpoilerTextBlock), null);

        private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (isTextHidden)
            {
                Paragraph p = new Paragraph();
                foreach (var run in HiddenContent)
                    p.Inlines.Add(run);

                this.HiddenRTB.Blocks.Add(p);
                this.HiddenTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.HiddenTextBorder.Background = new SolidColorBrush(Colors.Transparent);
                this.HiddenRTB.Visibility = Windows.UI.Xaml.Visibility.Visible;

                isTextHidden = false;
                e.Handled = true;
            }
        }
    }
}
