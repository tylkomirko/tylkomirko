using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Controls
{
    public sealed partial class AppHeader : UserControl
    {
        public AppHeader()
        {
            this.InitializeComponent();

            this.Loaded += AppHeader_Loaded;
            App.ApiService.NetworkStatusChanged += ApiService_NetworkStatusChanged;
        }

        private void ApiService_NetworkStatusChanged(object sender, WykopAPI.NetworkEventArgs e)
        {
            if (e.IsNetworkAvailable)
                DispatcherHelper.CheckBeginInvokeOnUI(() => DrawLogo());
            else
                DispatcherHelper.CheckBeginInvokeOnUI(() => DrawOfflineLogo());
        }

        private void AppHeader_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var currentPage = SimpleIoc.Default.GetInstance<INavigationService>().CurrentPageKey;

            var fill = Application.Current.Resources["AppHeaderSelectionBrush"] as SolidColorBrush;

            if (currentPage == "HashtagSelectionPage")
                this.HashTB.Foreground = fill;
            else if (currentPage == "AtNotificationsPage")
                this.AtTB.Foreground = fill;
            else if (currentPage == "ConversationsPage")
                this.PMTB.Foreground = fill;

            if (App.ApiService.IsNetworkAvailable)
                DrawRegularLogo();
            else
                DrawOfflineLogo();
        }

        private void DrawLogo()
        {
            // check current date and show appropriate logo
            var currentDate = DateTime.Now.Date;
            if (currentDate == new DateTime(2015, 2, 12) || currentDate == new DateTime(2016, 2, 4))
                DrawDonutLogo();
            else if (currentDate.Month == 2 && currentDate.Day == 14)
                DrawValentinesLogo();
            else
                DrawRegularLogo();
        }

        private void DrawRegularLogo()
        {
            var fill = Application.Current.Resources["LogoFill"] as SolidColorBrush;

            string insidePathData = "M 111.8 161.7c-9.4 4.6-18.6 9.4-28.1 13.6c-14-27.9-27.7-56-41.3-84.1C79.7 72.6 117.2 54.4 154.6 36   c14 27.9 27.6 56.1 41.4 84.1c-9.3 5-18.7 9.5-28.2 14c-11.6-23.2-22.9-46.6-34.4-69.8c-4.6 2.1-9.2 4.3-13.7 6.7   c11.4 23.3 23 46.5 34.2 69.9c-9.3 4.8-18.7 9.3-28.2 13.9c-11.5-23.2-22.8-46.6-34.4-69.8c-4.6 2-9.1 4.2-13.6 6.4   C88.6 115 100.9 138 111.8 161.7z";
            string outlinePathData = "M157.9 210H83.4c-18.9-2-39.1-2.8-55.8-12.9c-15.8-9.7-23.8-28.2-24.8-46.2C0.2 113.1-3.7 73.7 7.6 37    C18.5 7.4 52.8 1.4 80.4 0l76.5 0c20.2 2.2 42.4 2.6 59.2 15.5c20.8 15.7 21.8 43.6 23.5 67.3v51.5c-1.5 22.3-4.8 48.2-25.1 61.6    C198.1 207.6 177.2 208 157.9 210z M166.2 181.6c13.4-0.9 29.5-2 37.9-14.2c7.8-12.6 8.2-28.1 9-42.5c0.2-23.3 1.6-46.9-3.6-69.8    c-2.7-17.6-20.7-26.2-36.9-26.3C134.7 27.1 96.5 26 58.7 30c-16.3 0.8-27.9 15-29.6 30.6c-3.2 22-2.3 44.3-2 66.4    c0.6 15.1 1.3 31.9 11.4 44.1c10.2 10.1 25.6 9.8 38.9 10.8C107.1 182.7 136.7 182.9 166.2 181.6z";

            var outlinePath = new Path() { Height = 40, Width = 46, Stretch = Stretch.Uniform, Fill = fill };
            var insidePath = new Path() { Height = 28, Width = 32, Stretch = Stretch.Uniform, Fill = fill };
            
            BindingOperations.SetBinding(outlinePath, Path.DataProperty, new Binding() { Source = outlinePathData });
            BindingOperations.SetBinding(insidePath, Path.DataProperty, new Binding() { Source = insidePathData });

            Logo.Children.Clear();
            Logo.Children.Add(outlinePath);
            Logo.Children.Add(insidePath);
        }

        private void DrawOfflineLogo()
        {
            var fill = Application.Current.Resources["LogoFill"] as SolidColorBrush;

            string insidePathData = "M 119.2,54.7 C 111,54.7 104.3,61.4 104.3,69.6 L 113.9,111 124.3,111 134.1,69.6 C 134.1,61.4 127.4,54.7 119.2,54.7 Z";
            string insidePathData2 = "M 103.6,136.1 C 103.6,127.484 110.584,120.5 119.2,120.5 L 119.2,120.5 C 127.816,120.5 134.8,127.484 134.8,136.1 L 134.8,136.1 C 134.8,144.716 127.816,151.7 119.2,151.7 L 119.2,151.7 C 110.584,151.7 103.6,144.716 103.6,136.1 Z";
            string insidePathData3 = "M 92.6,72.3 L 92.3,71 92.3,69.6 C 92.3,67.8 92.5,66.1 92.8,64.4 79,69.6 67.1,78.8 58.5,90.4 58.4,90.6 58.2,90.8 58.1,90.9 58,91.2 58,91.4 58,91.7 58,92 58.1,92.3 58.3,92.5 L 58.7,93 67.9,103.1 C 68,103.4 68.3,103.7 68.6,103.9 68.8,104 69.1,104.1 69.4,104.1 69.8,104.1 70.2,103.9 70.5,103.7 70.6,103.5 70.7,103.4 70.8,103.3 77,95 85.4,88.2 95.3,84 L 92.6,72.3 Z";
            string insidePathData4 = "M 102.2,113.7 L 98.4,97.1 C 90.9,100.2 84.5,105 79.9,110.8 79.7,111.1 79.5,111.3 79.3,111.6 79.2,111.8 79.2,111.9 79.2,112.1 79.2,112.3 79.2,112.5 79.3,112.7 79.4,113 79.7,113.3 80,113.5 L 88.9,123 89.1,123.3 C 89.4,123.5 89.7,123.7 90.1,123.7 90.3,123.7 90.6,123.6 90.8,123.5 90.9,123.5 90.9,123.4 91,123.4 91.1,123.3 91.3,123.2 91.4,123.1 91.4,123.1 91.5,123.1 91.5,123 93.8,120.1 97.6,116.9 102.5,114.5 L 102.2,113.7 Z";
            string insidePathData5 = "M 181.5,91 C 181.4,90.8 181.2,90.6 181.1,90.5 172.2,78.5 159.8,69.1 145.5,64 145.9,65.8 146.1,67.7 146.1,69.6 L 146.1,71 145.8,72.4 143.2,83.4 C 153.6,87.6 162.5,94.5 168.9,103.2 169,103.3 169.1,103.5 169.2,103.6 169.5,103.9 169.9,104 170.3,104 170.6,104 170.9,103.9 171.1,103.8 171.4,103.6 171.7,103.4 171.8,103 L 181,93 181.4,92.5 C 181.5,92.3 181.7,92 181.7,91.7 181.6,91.4 181.6,91.2 181.5,91 Z";
            string insidePathData6 = "M 160.5,111.6 C 160.3,111.3 160.1,111.1 159.9,110.8 155,104.7 148.1,99.7 140,96.6 L 135.9,113.8 135.9,113.9 C 141.4,116.4 145.8,119.9 148.3,123.1 148.3,123.1 148.4,123.1 148.4,123.2 148.5,123.3 148.6,123.4 148.8,123.5 148.9,123.5 148.9,123.6 149,123.6 149.2,123.7 149.4,123.8 149.7,123.8 150.1,123.8 150.4,123.6 150.7,123.4 L 150.9,123.1 159.8,113.6 C 160.1,113.4 160.4,113.1 160.5,112.8 160.6,112.6 160.6,112.4 160.6,112.2 160.6,112 160.6,111.8 160.5,111.6 Z";
            string outlinePathData = "M 130.4,193 C 130.6,192.2 130.9,191.3 131.2,190.5 L 131.2,189.7 132.7,187.2 C 133,186.6 133.4,185.9 133.8,185.3 L 135.4,182.5 C 116.1,182.7 96.8,182.5 77.5,181.9 64.2,180.8 48.7,181.1 38.6,171.1 28.6,158.9 27.8,142.1 27.2,127 26.9,104.9 26,82.6 29.2,60.6 30.8,45 42.4,30.8 58.7,30 96.5,26.1 134.7,27.1 172.6,28.9 188.8,29 206.9,37.6 209.5,55.2 213.3,71.8 213.5,88.8 213.3,105.7 L 220.4,118 C 220.6,118.4 220.8,118.7 221,119.1 L 221,119.2 238.1,148.9 C 238.7,144 239.1,139.1 239.4,134.3 L 239.4,82.7 C 237.7,59 236.7,31.1 215.9,15.4 199.2,2.6 177.1,2.2 156.9,0 L 80.4,0 C 52.8,1.4 18.5,7.5 7.6,37 -3.6,73.8 0.3,113.2 2.7,151 3.7,169 11.7,187.5 27.5,197.2 44.3,207.2 64.4,208 83.4,210 L 131.3,210 C 129.2,204.6 128.9,198.7 130.4,193 Z";
            string trianglePathData = "M 244.3,204.1 C 246.2,200.8 246,196.9 244.1,193.9 L 205.5,127.1 205.5,127.1 C 205,126.1 204.4,125.2 203.6,124.4 201.7,122.5 199.1,121.5 196.5,121.5 193.9,121.5 191.4,122.5 189.4,124.4 188.7,125.1 188.1,125.9 187.7,126.7 L 187.7,126.7 149.1,193.5 149.1,193.5 C 148.5,194.4 148,195.4 147.7,196.5 146.3,201.8 149.4,207.3 154.7,208.7 155.5,208.9 156.4,209 157.3,209 157.4,209 157.5,209 157.6,209 L 234.8,209 234.8,209 C 235,209 235.3,209 235.5,209 239.1,209.1 242.4,207.3 244.3,204.1 Z M 196.6,200 C 191.9,200 188.2,196.2 188.2,191.6 188.2,187 192,183.2 196.6,183.2 201.3,183.2 205,187 205,191.6 205,196.3 201.3,200 196.6,200 Z M 199.6,177.7 L 193.4,177.7 187.7,153.1 C 187.7,148.2 191.7,144.3 196.5,144.3 201.4,144.3 205.3,148.3 205.3,153.1 L 199.6,177.7 Z";

            var insidePath = new Path() { Height = 13, Width = 23, Stretch = Stretch.Uniform, Fill = fill, Margin = new Thickness(0, -8, 0, 0) };
            var insidePath2 = new Path() { Height = 6, Width = 6, Stretch = Stretch.Uniform, Fill = fill, Margin = new Thickness(0, 14, 0, 0) };
            var insidePath3 = new Path() { Height = 9, Width = 7, Stretch = Stretch.Uniform, Fill = fill, Margin = new Thickness(-17.5, -10, 0, 0) };
            var insidePath4 = new Path() { Height = 9, Width = 4.5, Stretch = Stretch.Uniform, Fill = fill, Margin = new Thickness(-12, 4, 0, 0) };
            var insidePath5 = new Path() { Height = 9, Width = 7, Stretch = Stretch.Uniform, Fill = fill, Margin = new Thickness(17.5, -10, 0, 0) };
            var insidePath6 = new Path() { Height = 9, Width = 4.5, Stretch = Stretch.Uniform, Fill = fill, Margin = new Thickness(12, 4, 0, 0) };
            var outlinePath = new Path() { Height = 40, Width = 46, Stretch = Stretch.Uniform, Fill = fill };
            var trianglePath = new Path() { Height = 15, Width = 17, Stretch = Stretch.Uniform, Fill = fill, Margin = new Thickness(28.5, 25, 0, 0) };

            BindingOperations.SetBinding(insidePath, Path.DataProperty, new Binding() { Source = insidePathData });
            BindingOperations.SetBinding(insidePath2, Path.DataProperty, new Binding() { Source = insidePathData2 });
            BindingOperations.SetBinding(insidePath3, Path.DataProperty, new Binding() { Source = insidePathData3 });
            BindingOperations.SetBinding(insidePath4, Path.DataProperty, new Binding() { Source = insidePathData4 });
            BindingOperations.SetBinding(insidePath5, Path.DataProperty, new Binding() { Source = insidePathData5 });
            BindingOperations.SetBinding(insidePath6, Path.DataProperty, new Binding() { Source = insidePathData6 });
            BindingOperations.SetBinding(outlinePath, Path.DataProperty, new Binding() { Source = outlinePathData });
            BindingOperations.SetBinding(trianglePath, Path.DataProperty, new Binding() { Source = trianglePathData });

            Logo.Children.Clear();
            Logo.Children.Add(insidePath);
            Logo.Children.Add(insidePath2);
            Logo.Children.Add(insidePath3);
            Logo.Children.Add(insidePath4);
            Logo.Children.Add(insidePath5);
            Logo.Children.Add(insidePath6);
            Logo.Children.Add(outlinePath);
            Logo.Children.Add(trianglePath);
        }

        private void DrawDonutLogo()
        {
            var fill = Application.Current.Resources["LogoFill"] as SolidColorBrush;

            string outlinePathData = "M157.9 210H83.4c-18.9-2-39.1-2.8-55.8-12.9c-15.8-9.7-23.8-28.2-24.8-46.2C0.2 113.1-3.7 73.7 7.6 37    C18.5 7.4 52.8 1.4 80.4 0l76.5 0c20.2 2.2 42.4 2.6 59.2 15.5c20.8 15.7 21.8 43.6 23.5 67.3v51.5c-1.5 22.3-4.8 48.2-25.1 61.6    C198.1 207.6 177.2 208 157.9 210z M166.2 181.6c13.4-0.9 29.5-2 37.9-14.2c7.8-12.6 8.2-28.1 9-42.5c0.2-23.3 1.6-46.9-3.6-69.8    c-2.7-17.6-20.7-26.2-36.9-26.3C134.7 27.1 96.5 26 58.7 30c-16.3 0.8-27.9 15-29.6 30.6c-3.2 22-2.3 44.3-2 66.4    c0.6 15.1 1.3 31.9 11.4 44.1c10.2 10.1 25.6 9.8 38.9 10.8C107.1 182.7 136.7 182.9 166.2 181.6z";
            string insidePathData = "M 119.8,56.4 C 85.7,56.4 58,78.2 58,105 58,105.3 58,105.7 58,106 62.2,110.2 66.5,113.8 68.2,114.1 69.6,114.3 72.2,113.7 74.7,113.1 78.9,112.1 83.3,111 86.3,112.8 89.1,114.5 91.6,118.6 94.2,122.9 96.6,126.9 99.6,131.9 101.6,132.2 103.4,132.5 106.9,129.6 109.8,127.3 113.4,124.4 116.8,121.6 119.9,121.6 L 119.9,121.6 C 123,121.6 126.4,124.4 130,127.3 132.8,129.6 136.4,132.5 138.2,132.2 140.3,131.9 143.2,126.9 145.6,122.9 148.2,118.5 150.6,114.4 153.4,112.8 156.4,111 160.8,112.1 165,113.1 167.5,113.7 170,114.3 171.5,114.1 173.2,113.8 177.5,110.2 181.7,106 181.7,105.7 181.7,105.3 181.7,105 181.7,78.2 154,56.4 119.8,56.4 Z M 138.4,86.7 C 134,82.4 129.7,78.1 125.3,73.7 123.5,74.1 121.8,74.5 120.1,74.9 124.4,79.2 128.9,83.5 133.2,87.9 129.6,88.8 126,89.6 122.4,90.5 118,86.2 113.7,81.9 109.3,77.5 107.5,77.9 105.8,78.3 104.1,78.7 108.1,82.9 112.5,87 116.5,91.1 116.7,91.2 116.8,91.5 116.8,91.7 116.8,91.9 116.8,92.9 116.8,93.2 116.8,94.9 115.6,96.3 114,96.3 113.9,96.3 113.8,96.3 113.7,96.3 113.3,97.5 112.3,98.3 111.2,98.6 L 111.2,101.8 C 111.2,102.2 110.9,102.6 110.5,102.6 110.1,102.6 109.8,102.3 109.8,101.8 L 109.8,98.6 C 108.2,98.2 107,96.7 107,94.8 L 107,94 C 105,93.8 102.9,93.2 102.9,91.8 102.9,91.6 103,91.4 103.1,91.3 98.8,87.1 94.6,82.9 90.4,78.7 104.7,75.3 119,71.9 133.3,68.5 138.7,73.7 143.9,78.9 149.1,84.1 145.6,85.1 142,85.9 138.4,86.7 Z";
            string insidePathData2 = "M 172.1 120.1c-2.2 0.4-5-0.3-8.1-1c-3-0.7-7-1.7-8.5-0.8c-1.7 1-4 4.9-6 8.3c-3.3 5.5-6.6 11.2-10.9 11.8  c-0.2 0-0.5 0-0.7 0c-3.3 0-6.9-2.9-10.7-6c-2.6-2.1-5.8-4.7-7.4-4.7v0c-1.6 0-4.8 2.6-7.4 4.7c-3.8 3.1-7.3 6-10.7 6  c-0.2 0-0.5 0-0.7 0c-4.2-0.6-7.6-6.3-10.9-11.8c-2-3.4-4.3-7.2-6-8.3c-1.5-0.9-5.6 0.1-8.5 0.8c-3 0.7-5.8 1.4-8.1 1  c-2.1-0.4-5.3-2.6-8.3-5.3c5.8 22.1 30.7 38.7 60.6 38.7c29.9 0 54.8-16.6 60.6-38.7C177.4 117.5 174.2 119.8 172.1 120.1z";

            var outlinePath = new Path() { Height = 40, Width = 46, Stretch = Stretch.Uniform, Fill = fill };
            var insidePath = new Path() { Width = 25, Stretch = Stretch.Uniform, Fill = fill, Margin = new Thickness(0, -4, 0, 0) };
            var insidePath2 = new Path() { Width = 25, Stretch = Stretch.Uniform, Fill = fill, Margin = new Thickness(0, 13, 0, 0) };

            BindingOperations.SetBinding(outlinePath, Path.DataProperty, new Binding() { Source = outlinePathData });
            BindingOperations.SetBinding(insidePath, Path.DataProperty, new Binding() { Source = insidePathData });
            BindingOperations.SetBinding(insidePath2, Path.DataProperty, new Binding() { Source = insidePathData2 });

            Logo.Children.Clear();
            Logo.Children.Add(outlinePath);
            Logo.Children.Add(insidePath);
            Logo.Children.Add(insidePath2);
        }

        private void DrawValentinesLogo()
        {
            var fill = Application.Current.Resources["LogoFill"] as SolidColorBrush;

            string outlinePathData = "M 153.4,27 C 182.4,27 206,50.5 206,79.5 206.1,146.3 116.6,182.1 116.6,182.1 116.6,182.1 27.1,146.5 27,79.8 27,50.8 50.5,27.2 79.5,27.1 79.5,27.1 79.5,27.1 79.6,27.1 94,27.1 107,32.9 116.5,42.2 126,32.8 139,27 153.4,27 153.4,27 153.4,27 153.4,27 M 153.4,0 L 153.4,0 C 140.3,0 127.7,3.2 116.4,9.1 105.2,3.2 92.6,0.1 79.6,0.1 L 79.5,0.1 C 35.6,0.1 0,35.9 0,79.8 0,115.8 19.2,149.5 55.4,177.4 80.5,196.7 105.6,206.8 106.7,207.2 L 116.7,211.2 126.7,207.2 C 127.8,206.8 152.9,196.6 177.9,177.2 214,149.3 233.1,115.5 233,79.5 233,35.7 197.3,0 153.4,0 L 153.4,0 Z";
            string insidePathData = "M 110.7,144.9 C 103.3,148.5 96,152.3 88.4,155.7 77.4,133.6 66.5,111.4 55.7,89.1 85.2,74.4 114.9,60 144.5,45.5 155.6,67.6 166.4,89.9 177.3,112.1 170,116 162.5,119.6 155,123.1 145.8,104.7 136.9,86.2 127.8,67.8 124.1,69.5 120.5,71.2 117,73.1 126,91.6 135.2,109.9 144.1,128.4 136.7,132.2 129.3,135.8 121.8,139.4 112.7,121 103.7,102.5 94.6,84.2 91,85.8 87.4,87.5 83.8,89.2 92.4,108 102.1,126.2 110.7,144.9 Z";

            var outlinePath = new Path() { Height = 40, Width = 46, Stretch = Stretch.Uniform, Fill = fill };
            var insidePath = new Path() { Width = 20, Stretch = Stretch.Uniform, Fill = fill, Margin = new Thickness(0, -1, 0, 0) };

            BindingOperations.SetBinding(outlinePath, Path.DataProperty, new Binding() { Source = outlinePathData });
            BindingOperations.SetBinding(insidePath, Path.DataProperty, new Binding() { Source = insidePathData });

            Logo.Children.Clear();
            Logo.Children.Add(outlinePath);
            Logo.Children.Add(insidePath);
        }

        public void PlayAnimation()
        {
            LogoAnimation.Begin();
            NotificationsAnimation.Begin();
        }
    }
}
