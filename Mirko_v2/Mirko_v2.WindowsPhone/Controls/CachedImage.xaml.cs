using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Controls
{
    public sealed partial class CachedImage : UserControl
    {
        public CachedImage()
        {
            this.InitializeComponent();
        }

        #region Uri depedency property
        public string Uri
        {
            get { return (string)GetValue(UriProperty); }
            set { SetValue(UriProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Uri.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UriProperty =
            DependencyProperty.Register("Uri", typeof(string), typeof(CachedImage), new PropertyMetadata(null, new PropertyChangedCallback(UriChanged)));

        private static async void UriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var url = e.NewValue as string;
            if (url == null) return;

            var control = d as CachedImage;
            var fullURL = (control.DataContext as EmbedViewModel).EmbedData.URL;
            var cacheVM = SimpleIoc.Default.GetInstance<CacheViewModel>();

            control.Ring.IsActive = true;
            control.Ring.Visibility = Visibility.Visible;
            var stream = await cacheVM.GetImageStream(url, fullURL);

            if(stream != null)
            {
                var bitmap = new BitmapImage();
                bitmap.SetSource(stream);
                control.Image.Source = bitmap;
                control.Ring.Visibility = Visibility.Collapsed;

                //stream.Dispose(); // when to dispose?
            }
            else
            {
                control.Ring.Visibility = Visibility.Collapsed;
                control.Ring.IsActive = false;
                control.Image.Opacity = 1.0;
                control.Image.Visibility = Visibility.Collapsed;
                control.Visibility = Visibility.Collapsed; // none of this really works.
            }           
        }
        #endregion
    }
}
