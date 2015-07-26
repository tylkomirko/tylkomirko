using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Mirko_v2.ViewModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using WykopAPI.Models;
using System;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Controls
{
    public sealed partial class CachedImage : UserControl
    {
        private static CacheViewModel Cache = null;

        public CachedImage()
        {
            this.InitializeComponent();

            if(Cache == null)
                Cache = SimpleIoc.Default.GetInstance<CacheViewModel>();
        }

        private async Task LoadImage(string previewURL, string fullURL = null)
        {
            await DispatcherHelper.RunAsync(async () =>
            {
                Image.Visibility = Visibility.Collapsed;
                Grid.Visibility = Visibility.Visible;
                Ring.IsActive = true;
                Ring.Visibility = Visibility.Visible;

                var stream = await Cache.GetImageStream(previewURL, fullURL);

                if (stream != null)
                {
                    var bitmap = new BitmapImage() { CreateOptions = BitmapCreateOptions.None };
                    bitmap.SetSource(stream);

                    Image.Source = bitmap;
                    Grid.Visibility = Visibility.Visible;
                    Ring.Visibility = Visibility.Collapsed;
                    Image.Visibility = Visibility.Visible;

                    stream.Dispose();
                    stream = null;
                }
                else
                {
                    Visibility = Visibility.Collapsed;
                }
            });
        }

        public new Visibility Visibility
        {
            get { return (Visibility)GetValue(VisibilityProperty); }
            set { SetValue(VisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Visibility.  This enables animation, styling, binding, etc...
        public static readonly new DependencyProperty VisibilityProperty =
            DependencyProperty.Register("Visibility", typeof(Visibility), typeof(CachedImage), new PropertyMetadata(Visibility.Visible, VisibilityChanged));

        private static async void VisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CachedImage;

            if ((Visibility)e.NewValue == Visibility.Visible)
            {
                if (control.Embed != null)
                    await control.LoadImage(control.Embed.PreviewURL, control.Embed.URL);
                else if (control.URL != null)
                    await control.LoadImage(control.URL);
            }
            else
            {
                control.Image.Visibility = Visibility.Collapsed;
                control.Ring.Visibility = Visibility.Collapsed;
                control.Grid.Visibility = Visibility.Collapsed;
            }
        }

        public Embed Embed
        {
            get { return (Embed)GetValue(EmbedProperty); }
            set { SetValue(EmbedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Embed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EmbedProperty =
            DependencyProperty.Register("Embed", typeof(Embed), typeof(CachedImage), new PropertyMetadata(null, EmbedChanged));

        private static async void EmbedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var embed = e.NewValue as Embed;
            if (embed == null) return;

            var control = d as CachedImage;
            if (control.Visibility == Visibility.Visible)
                await control.LoadImage(embed.PreviewURL, embed.URL);
        }

        public string URL
        {
            get { return (string)GetValue(URLProperty); }
            set { SetValue(URLProperty, value); }
        }

        // Using a DependencyProperty as the backing store for URL.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty URLProperty =
            DependencyProperty.Register("URL", typeof(string), typeof(CachedImage), new PropertyMetadata(null, URLChanged));

        private static async void URLChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var url = e.NewValue as string;
            if (url == null) return;

            var control = d as CachedImage;
            if (control.Visibility == Visibility.Visible)
                await control.LoadImage(url);
        }       
    }
}
