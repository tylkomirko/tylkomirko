using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using MetroLog;
using Mirko_v2.Utils;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Mirko_v2.ViewModel
{
    public class CacheViewModel : ViewModelBase
    {
        private readonly TimeSpan FileLifeSpan = new TimeSpan(24, 0, 0);
        private Timer SaveTimer = null;
        private readonly ILogger Logger = null;

        // image cache
        private StorageFolder ImageCacheFolder = null;

        public CacheViewModel()
        {
            Logger = LogManagerFactory.DefaultLogManager.GetLogger<CacheViewModel>();
            SaveTimer = new Timer(SaveTimerCallback, null, 45*1000, 0); // 45 seconds
        }

        private async void SaveTimerCallback(object state)
        {
            Logger.Info("Saving cache.");
            await Save();

            SaveTimer.Dispose(); // effectivly this is a one-shot timer
            SaveTimer = null;
        }

        #region Properties
        private ObservableCollectionEx<string> _popularHashtags = null;
        public ObservableCollectionEx<string> PopularHashtags
        {
            get { return _popularHashtags ?? (_popularHashtags = new ObservableCollectionEx<string>()); }
        }

        private ObservableCollectionEx<string> _hashtagSuggestions = null;
        public ObservableCollectionEx<string> HashtagSuggestions
        {
            get { return _hashtagSuggestions ?? (_hashtagSuggestions = new ObservableCollectionEx<string>()); }
        }

        private string _hashtagScratchpad = "#";
        public string HashtagScratchpad
        {
            get { return _hashtagScratchpad; }
            set { Set(() => HashtagScratchpad, ref _hashtagScratchpad, value); }
        }
        #endregion

        #region Commands
        private RelayCommand _initCommand = null;
        public RelayCommand InitCommand
        {
            get { return _initCommand ?? (_initCommand = new RelayCommand(ExecuteInitCommand)); }
        }

        private async void ExecuteInitCommand()
        {
            bool needToDownload = false;
            var tempFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;

            // PopularTags
            try
            {
                var file = await tempFolder.GetFileAsync("PopularTags");
                var props = await file.GetBasicPropertiesAsync();
                if (DateTime.Now - props.DateModified > FileLifeSpan)
                {
                    needToDownload = true;
                }
                else
                {
                    var fileContent = await FileIO.ReadLinesAsync(file);
                    Logger.Info(fileContent.Count + " entries in PopularTags");
                    if (fileContent.Count == 0)
                    {
                        needToDownload = true;
                    }
                    else
                    {
                        PopularHashtags.Clear();
                        PopularHashtags.AddRange(fileContent);
                    }
                }
            } 
            catch(Exception)
            {
                needToDownload = true;
            }

            if (needToDownload)
            {
                Logger.Info("Downloading PopularHashtags.");
                var data = await App.ApiService.getPopularTags();
                if (data != null)
                {
                    PopularHashtags.Clear();
                    PopularHashtags.AddRange(data.Select(x => x.HashtagName));
                    data = null;
                }
                else
                {
                    Logger.Warn("Downloading PopularHashtags failed.");
                }
            }            
        }

        private async Task Save()
        {
            var tempFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;

            // PopularTags
            var file = await tempFolder.CreateFileAsync("PopularTags", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteLinesAsync(file, PopularHashtags);
            Logger.Info("Saved PopularHashtags, " + PopularHashtags.Count + " entries.");
        }
        #endregion

        #region Image cache
        private async Task<InMemoryRandomAccessStream> DrawGIFOverlay(IBuffer buffer)
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

        private async Task<InMemoryRandomAccessStream> DrawVideoOverlay(IBuffer buffer)
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

        public async Task<IRandomAccessStream> GetImageStream(string previewURL, string fullURL)
        {
            if (previewURL == null || fullURL == null) return null;

            Uri uri = new Uri(previewURL);

            string fileName = System.IO.Path.GetFileName(uri.AbsolutePath);
            if (fileName.Length > 255) // sometimes, file names are really long
                fileName = Cryptography.EncodeMD5(fileName);

            previewURL = previewURL.Replace("w400gif.jpg", "w400.jpg"); // download preview image without nasty GIF logo on it.

            if (ImageCacheFolder == null)
            {
                var localFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
                ImageCacheFolder = await localFolder.CreateFolderAsync("ImageCache", CreationCollisionOption.OpenIfExists);
            }

            StorageFile file = null;

            // try to read from file
            try
            {
                file = await ImageCacheFolder.GetFileAsync(fileName);
                var stream = await file.OpenAsync(FileAccessMode.Read);
                Messenger.Default.Send<NotificationMessage<ulong>>(new NotificationMessage<ulong>(stream.Size, "ImgCacheHit"));

                return stream;
            }
            catch(FileNotFoundException)
            {
                // well, that's just fine. there's no need to do anything - image will be downloaded later on.
            }
            catch (Exception e)
            {
                Logger.Error("Error reading file. ", e);
            }

            try
            {
                using (var response = await App.ApiService.httpClient.GetAsync(previewURL))
                {
                    var pixels = await response.Content.ReadAsByteArrayAsync();
                    InMemoryRandomAccessStream stream = null;
                    if (fullURL.EndsWith(".gif") || fullURL.Contains("gfycat"))
                        stream = await DrawGIFOverlay(pixels.AsBuffer());
                    else if (fullURL.Contains("youtube") || fullURL.Contains("youtu.be"))
                        stream = await DrawVideoOverlay(pixels.AsBuffer());
                    else
                        stream = await ReadImage(pixels.AsBuffer());

                    // and now save
                    file = await ImageCacheFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                    using (var fileStream = await file.OpenStreamForWriteAsync())
                    {
                        var saveStream = stream.AsStream();
                        saveStream.Position = 0;
                        await saveStream.CopyToAsync(fileStream);
                    }

                    stream.Seek(0);
                    return stream;
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error downloading image.", e);
                return null;
            }
        }
        #endregion
    }
}
