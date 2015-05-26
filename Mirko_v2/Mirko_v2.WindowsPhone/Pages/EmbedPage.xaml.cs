using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Pages
{
    public sealed partial class EmbedPage : UserControl
    {
        public EmbedPage()
        {
            this.InitializeComponent();
        }

        private void ImageScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            img.Height = Window.Current.Bounds.Height;
            img.Width = Window.Current.Bounds.Width;

            var x = ImageScrollViewer.ScrollableWidth / 2;
            var y = ImageScrollViewer.ScrollableHeight / 2;

#pragma warning disable 0618
            ImageScrollViewer.ScrollToHorizontalOffset(x);
            ImageScrollViewer.ScrollToVerticalOffset(y);
#pragma warning restore 0618
        }

        private void img_Holding(object sender, HoldingRoutedEventArgs e)
        {
            var mf = this.Resources["SaveFlyout"] as MenuFlyout;
            mf.ShowAt(img);
        }

        private void img_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
#pragma warning disable 0618
            ImageScrollViewer.ZoomToFactor(2.0F);

            var x = ImageScrollViewer.ScrollableWidth / 2;
            var y = ImageScrollViewer.ScrollableHeight / 2;

            ImageScrollViewer.ScrollToHorizontalOffset(x);
            ImageScrollViewer.ScrollToVerticalOffset(y);
#pragma warning restore 0618
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void img_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var bitmap = args.NewValue as BitmapImage;
            if (bitmap == null) return;

            var xScale = (bitmap.PixelWidth * 4) / Window.Current.Bounds.Width;
            var yScale = (bitmap.PixelHeight * 4) / Window.Current.Bounds.Height;
            var maxScale = (new List<double>(2) { xScale, yScale }).Max();
            ImageScrollViewer.MaxZoomFactor = (float)maxScale;

            Ring.IsActive = false;
            Ring.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}
