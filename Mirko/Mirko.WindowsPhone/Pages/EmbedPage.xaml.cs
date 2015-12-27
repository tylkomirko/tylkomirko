using Mirko.Utils;
using System;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko.Pages
{
    public sealed partial class EmbedPage : Page
    {
        private bool ImageOpened = false;

        public EmbedPage()
        {
            this.InitializeComponent();

            this.Loaded += (s, e) =>
            {
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape | 
                    DisplayOrientations.LandscapeFlipped | DisplayOrientations.Portrait | 
                    DisplayOrientations.PortraitFlipped;

                StatusBarManager.HideStatusBar();

                if ((Image.Content as Image)?.Source != null)
                    ImageOpened = true;
                else
                    ImageScrollViewer.ChangeView(0.0, 0.0, 0.3f, false);
            };

            this.Unloaded += (s, e) =>
            {
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait | DisplayOrientations.PortraitFlipped;
                StatusBarManager.ShowStatusBar();
                ImageOpened = false;
            };

            this.ImageScrollViewer.SizeChanged += (s, e) =>
            {
                if (ImageOpened)
                    CalculateZoomFactors();
            };

            this.Image.ImageOpened += (s, e) =>
            {
                CalculateZoomFactors();
                ImageOpened = true;
            };

            this.Image.DoubleTapped += (s, e) =>
            {
                if (ImageOpened)
                    ZoomImage();
            };
        }

        private void CalculateZoomFactors()
        {
            var bmp = (Image.Content as Image)?.Source as WriteableBitmap;
            if (bmp == null) return;

            var ratioX = ImageScrollViewer.ViewportWidth / (double)bmp.PixelWidth;
            var ratioY = ImageScrollViewer.ViewportHeight / (double)bmp.PixelHeight;

            var zoom = Math.Min(ratioX, ratioY);
            var zoomMin = Math.Max(0.98 * zoom, 0.1); // it can't be smaller than 0.1. otherwise it throws.
            var zoomMax = 2.5 * Math.Max(ratioX, ratioY);
            ImageScrollViewer.MinZoomFactor = (float)zoomMin;
            ImageScrollViewer.MaxZoomFactor = (float)zoomMax;

            // fuck you MS
            Windows.System.Threading.ThreadPoolTimer.CreateTimer(async (source) =>
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ImageScrollViewer.ChangeView(0.0, 0.0, (float)zoom, false);
                });
            }, TimeSpan.FromMilliseconds(10));
        }

        private void ZoomImage()
        {
            var zoom = 2.0f * ImageScrollViewer.ZoomFactor;
            if (zoom > ImageScrollViewer.MaxZoomFactor)
                zoom = ImageScrollViewer.MaxZoomFactor;

            // fuck you MS
            Windows.System.Threading.ThreadPoolTimer.CreateTimer(async (source) =>
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ImageScrollViewer.ChangeView(null, null, zoom, false);
                    ImageScrollViewer.ChangeView(ImageScrollViewer.ScrollableWidth / 2, ImageScrollViewer.ScrollableHeight / 2, null, false);
                });
            }, TimeSpan.FromMilliseconds(10));
        }

        private void Image_Holding(object sender, HoldingRoutedEventArgs e)
        {
            SaveFlyout.ShowAt(Image);
        }
    }
}
