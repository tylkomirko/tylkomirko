using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using WykopAPI.Models;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mirko_v2.Controls
{
    public sealed partial class CachedImage : UserControl
    {
        public CachedImage()
        {
            this.InitializeComponent();
        }

        public void ImageLoaded()
        {
            this.ImageFadeIn.Begin();

            this.Ring.IsActive = false;
            this.Ring.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private static async Task<InMemoryRandomAccessStream> DrawGIFOverlay(IBuffer buffer)
        {
            var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(buffer);
            stream.Seek(0);
            var image = await new WriteableBitmap(1, 1).FromStream(stream);

            var gifOverlay = await new WriteableBitmap(1, 1).FromContent(new Uri("ms-appx:///Assets/gif_icon.png"));

            var startPoint = new Point()
            {
                X = (image.PixelWidth - gifOverlay.PixelWidth) / 2,
                Y = (image.PixelHeight - gifOverlay.PixelHeight) / 2,
            };

            image.Blit(new Rect(startPoint.X, startPoint.Y, gifOverlay.PixelWidth, gifOverlay.PixelHeight),
                gifOverlay,
                new Rect(0, 0, gifOverlay.PixelWidth, gifOverlay.PixelHeight),
                WriteableBitmapExtensions.BlendMode.Alpha);

            await image.ToStreamAsJpeg(stream);
            return stream;
        }

        private static async Task<InMemoryRandomAccessStream> DrawVideoOverlay(IBuffer buffer)
        {
            var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(buffer);
            stream.Seek(0);
            var image = await new WriteableBitmap(1, 1).FromStream(stream);

            var gifOverlay = await new WriteableBitmap(1, 1).FromContent(new Uri("ms-appx:///Assets/video_icon.png"));

            var startPoint = new Point()
            {
                X = (image.PixelWidth - gifOverlay.PixelWidth) / 2,
                Y = (image.PixelHeight - gifOverlay.PixelHeight) / 2,
            };

            image.Blit(new Rect(startPoint.X, startPoint.Y, gifOverlay.PixelWidth, gifOverlay.PixelHeight),
                gifOverlay,
                new Rect(0, 0, gifOverlay.PixelWidth, gifOverlay.PixelHeight),
                WriteableBitmapExtensions.BlendMode.Alpha);

            await image.ToStreamAsJpeg(stream);
            return stream;
        }

        private static async Task<InMemoryRandomAccessStream> ReadImage(IBuffer buffer)
        {
            var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(buffer);
            stream.Seek(0);

            return stream;
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

            string fileName = "";
            Uri uri = new Uri(url);
            fileName = System.IO.Path.GetFileName(uri.AbsolutePath);

            url = url.Replace("w400gif.jpg", "w400.jpg"); // download preview image without nasty GIF logo on it.

            var control = d as CachedImage;
            var fullURL = (control.DataContext as EmbedViewModel).EmbedData.URL;
            bool downloadFromNet = false;

            var localFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            var imgFolder = await localFolder.CreateFolderAsync("ImageCache", CreationCollisionOption.OpenIfExists);
            StorageFile file = null;

            // check if file exists
            var cachedImages = SimpleIoc.Default.GetInstance<CacheViewModel>().CachedImages;
            if (cachedImages.Contains(fileName))
            {
                // read from file
                try
                {
                    file = await imgFolder.GetFileAsync(fileName);

                    using (var stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.SetSource(stream);
                        control.Image.Source = bitmap;
                    }
                }
                catch (Exception)
                {
                    downloadFromNet = true;
                }
            }
            else
            {
                downloadFromNet = true;
            }

            if(downloadFromNet && !App.ApiService.IsNetworkAvailable)
            {
                control.Ring.Visibility = Visibility.Collapsed;
                control.Ring.IsActive = false;
                control.Image.Opacity = 1.0;
                control.Image.Visibility = Visibility.Collapsed;
                control.Visibility = Visibility.Collapsed; // none of this really works.
            }
            else if(downloadFromNet)
            {
                control.Ring.Visibility = Visibility.Visible;
                control.Ring.IsActive = true;
                control.Image.Opacity = 0.0;

                try
                {
                    using (var response = await App.ApiService.httpClient.GetAsync(url))
                    {
                        var pixels = await response.Content.ReadAsByteArrayAsync();
                        InMemoryRandomAccessStream stream = null;
                        if (fullURL.EndsWith(".gif") || fullURL.Contains("gfycat"))
                            stream = await DrawGIFOverlay(pixels.AsBuffer());
                        else if (fullURL.Contains("youtube") || fullURL.Contains("youtu.be"))
                            stream = await DrawVideoOverlay(pixels.AsBuffer());
                        else
                            stream = await ReadImage(pixels.AsBuffer());

                        var bitmap = new BitmapImage();
                        bitmap.SetSource(stream);

                        control.Image.Source = bitmap;
                        control.ImageLoaded();

                        // and now save
                        file = await imgFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                        using (var saveStream = stream.AsStream())
                        using (var fileStream = await file.OpenStreamForWriteAsync())
                        {
                            saveStream.Position = 0;
                            await saveStream.CopyToAsync(fileStream);

                            cachedImages.Add(fileName);
                        }

                        stream.Dispose();
                    }
                }
                catch (Exception)
                {
                    // display error of a kind.
                    control.Image.Visibility = Visibility.Collapsed;
                }
            }            
        }
        #endregion
    }
}
