﻿using GalaSoft.MvvmLight.Ioc;
using Mirko.Utils;
using Mirko.ViewModel;
using System;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

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

                if (SimpleIoc.Default.GetInstance<SettingsViewModel>().SelectedTheme == ElementTheme.Dark)
                    ImageScrollViewer.Background = new SolidColorBrush(Colors.Black);
                else
                    ImageScrollViewer.Background = new SolidColorBrush(Colors.White);
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
                StatusBarManager.HideProgress();
                StatusBarManager.HideStatusBar();

                CalculateZoomFactors();
                ImageOpened = true;
            };

            this.Image.DoubleTapped += (s, e) => ZoomImage();
        }

        private void CalculateZoomFactors()
        {
            var ratioX = ImageScrollViewer.ViewportWidth / Image.ActualWidth;
            var ratioY = ImageScrollViewer.ViewportHeight / Image.ActualHeight;

            var zoom = Math.Min(ratioX, ratioY);
            var zoomMin = Math.Max(0.97 * zoom, 0.1); // it can't be smaller than 0.1. otherwise it throws.
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

        private void Image_OpenFlyout(object sender, RoutedEventArgs e)
        {
            var holding = e as HoldingRoutedEventArgs;
            var rightTap = e as RightTappedRoutedEventArgs;

            if (holding == null && rightTap == null) return;

            if (holding != null &&
                (holding.HoldingState == Windows.UI.Input.HoldingState.Completed ||
                 holding.HoldingState == Windows.UI.Input.HoldingState.Canceled))
                return;

            var mf = FlyoutBase.GetAttachedFlyout(Image);
            mf.ShowAt(Image);

            if (holding != null)
                holding.Handled = true;
            else
                rightTap.Handled = true;
        }
    }
}
