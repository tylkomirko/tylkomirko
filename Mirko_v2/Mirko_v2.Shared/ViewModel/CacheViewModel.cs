using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MetroLog;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using WykopSDK.Utils;

namespace Mirko_v2.ViewModel
{
    public class CacheViewModel : ViewModelBase
    {
        private readonly TimeSpan FileLifeSpan = new TimeSpan(12, 0, 0);
        private readonly ILogger Logger = null;
        private readonly StorageFolder TempFolder = null;
        private StorageFolder ImageCacheFolder = null;

        public Action GetPopularHashtags = null;
        public Action GetObservedUsers = null;

        public CacheViewModel()
        {
            TempFolder = ApplicationData.Current.TemporaryFolder;
            Logger = LogManagerFactory.DefaultLogManager.GetLogger<CacheViewModel>();

            GetPopularHashtags = new Action(async () => await DownloadPopularHashtags());
            GetObservedUsers = new Action(async () =>
            {
                var users = await Task.Run(App.WWWService.GetObservedUsers);

                if (users != null)
                {
                    ObservedUsers.Clear();
                    ObservedUsers.AddRange(users.Select(x => "@" + x));
                }
            });

            Messenger.Default.Register<NotificationMessage>(this, ReadMessage);
            Messenger.Default.Register<NotificationMessage<string>>(this, ReadMessage);
        }

        private async void ReadMessage(NotificationMessage obj)
        {
            if (obj.Notification == "Logout")
                await Logout();
            else if (obj.Notification == "Update ObservedHashtags")
                await DownloadObservedHashtags();
            else if (obj.Notification == "Save ObservedHashtags")
                await SaveObservedHashtags();
            else if (obj.Notification == "Delete ObservedHashtags")
                await DeleteObservedHashtags();
        }

        private async void ReadMessage(NotificationMessage<string> obj)
        {
            if (obj.Notification == "Add ObservedUser")
            {
                var username = obj.Content;

                await DispatcherHelper.RunAsync(() =>
                {
                    ObservedUsers.Add(username);
                    ObservedUsers.Sort();
                });
                await WykopSDK.WykopSDK.LocalStorage.SaveObservedUsers(ObservedUsers);
            }
            else if(obj.Notification == "Remove ObservedUser")
            {
                var username = obj.Content;

                await DispatcherHelper.RunAsync(() => ObservedUsers.Remove(username));
                await WykopSDK.WykopSDK.LocalStorage.SaveObservedUsers(ObservedUsers);
            }
        }

        private async Task Logout()
        {
            await DispatcherHelper.RunAsync(() => ObservedHashtags.Clear());
            await DeleteObservedHashtags();

            await DispatcherHelper.RunAsync(() => ObservedUsers.Clear());
            await WykopSDK.WykopSDK.LocalStorage.DeleteObservedUsers();

            await WykopSDK.WykopSDK.LocalStorage.DeleteConversations();
            await WykopSDK.WykopSDK.LocalStorage.DeleteBlacklists();
        }

        #region ObservedHashtags
        private ObservableCollectionEx<string> _observedHashtags;
        public ObservableCollectionEx<string> ObservedHashtags
        {
            get { return _observedHashtags ?? (_observedHashtags = new ObservableCollectionEx<string>()); }
        }

        private async Task DownloadObservedHashtags()
        {
            var needToDownload = false;
            try
            {
                var file = await TempFolder.GetFileAsync("ObservedHashtags");
                var props = await file.GetBasicPropertiesAsync();
                if (DateTime.Now - props.DateModified > FileLifeSpan)
                {
                    needToDownload = true;
                }
                else
                {
                    var fileContent = await FileIO.ReadLinesAsync(file);
                    ObservedHashtags.Clear();
                    ObservedHashtags.AddRange(fileContent);
                }
            }
            catch (FileNotFoundException)
            {
                Logger.Error("ObservedHashtags not found.");
                needToDownload = true;
            }
            catch (Exception e)
            {
                Logger.Error("Couldn't read ObservedHashtags.", e);
                needToDownload = true;
            }

            if (needToDownload)
            {
                var data = await App.ApiService.getUserObservedTags();
                if (data != null)
                {
                    ObservedHashtags.Clear();
                    ObservedHashtags.AddRange(data);
                    data = null;

                    await SaveObservedHashtags();
                }
            }
        }

        private async Task SaveObservedHashtags()
        {
            if (ObservedHashtags.Count == 0)
                return;

            try
            {
                var file = await TempFolder.CreateFileAsync("ObservedHashtags", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteLinesAsync(file, ObservedHashtags);
            }
            catch (Exception e)
            {
                Logger.Error("Couldn't save ObservedHashtags.", e);
            }
        }

        private async Task DeleteObservedHashtags()
        {
            try
            {
                var file = await TempFolder.GetFileAsync("ObservedHashtags");
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception e)
            {
                Logger.Error("Couldn't delete ObservedHashtags.", e);
            }
        }
        #endregion ObservedHashtags

        #region PopularHashtags
        private ObservableCollectionEx<string> _popularHashtags = null;
        public ObservableCollectionEx<string> PopularHashtags
        {
            get { return _popularHashtags ?? (_popularHashtags = new ObservableCollectionEx<string>()); }
        }

        private async Task DownloadPopularHashtags()
        {
            bool needToDownload = false;

            try
            {
                var file = await TempFolder.GetFileAsync("PopularHashtags");
                var props = await file.GetBasicPropertiesAsync();
                if (DateTime.Now - props.DateModified > FileLifeSpan)
                {
                    needToDownload = true;
                }
                else
                {
                    var fileContent = await FileIO.ReadLinesAsync(file);
                    Logger.Info(fileContent.Count + " entries in PopularHashtags");
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
            catch(FileNotFoundException)
            {
                Logger.Error("PopularHashtags not found.");
                needToDownload = true;
            }
            catch (Exception e)
            {
                Logger.Error("Couldn't read PopularHashtags.", e);
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

                    await SavePopularHashtags();
                }
                else
                {
                    Logger.Warn("Downloading PopularHashtags failed.");
                }
            } 
        }

        private async Task SavePopularHashtags()
        {
            try
            {
                var file = await TempFolder.CreateFileAsync("PopularHashtags", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteLinesAsync(file, PopularHashtags);
                Logger.Info("Saved PopularHashtags, " + PopularHashtags.Count + " entries.");
            }
            catch(Exception e)
            {
                Logger.Error("Couldn't save PopularHashtags.", e);
            }
          
        }
        #endregion

        #region Users
        private ObservableCollectionEx<string> _observedUsers = null;
        public ObservableCollectionEx<string> ObservedUsers
        {
            get { return _observedUsers ?? (_observedUsers = new ObservableCollectionEx<string>()); }
        }

        private ObservableCollectionEx<string> _tempUsers = null;
        public ObservableCollectionEx<string> TempUsers
        {
            get { return _tempUsers ?? (_tempUsers = new ObservableCollectionEx<string>()); }
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

        private string GetFilename(string previewURL)
        {
            if (previewURL == null) return null;

            Uri uri = new Uri(previewURL);

            string fileName = System.IO.Path.GetFileName(uri.AbsolutePath);
            if (fileName.Length > 255) // sometimes, file names are really long
                fileName = Cryptography.EncodeMD5(fileName);

            return fileName;
        }

        public async Task<Uri> GetImageUri(string previewURL, string fullURL = null)
        {
            /*
            var r = new Random();
            if (r.Next(1, 8) % 2 == 0)
            {
                Debug.WriteLine("GetImageStream returns NULL");
                return null;
            }*/

            if (previewURL == null) return null;

            string fileName = GetFilename(previewURL);

            previewURL = previewURL.Replace("w400gif.jpg", "w400.jpg"); // download preview image without nasty GIF logo on it.

            if (ImageCacheFolder == null)
                ImageCacheFolder = await TempFolder.CreateFolderAsync("ImageCache", CreationCollisionOption.OpenIfExists);

            StorageFile file = null;

            // try to read from file
            try
            {
                file = await ImageCacheFolder.GetFileAsync(fileName);
                Messenger.Default.Send<NotificationMessage<ulong>>(new NotificationMessage<ulong>(0, "ImgCacheHit"));

                return new Uri(string.Format("ms-appdata:///temp/ImageCache/{0}", file.Name));
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
                    if(fullURL == null)
                        stream = await ReadImage(pixels.AsBuffer());
                    else if (fullURL.EndsWith(".gif") || fullURL.Contains("gfycat"))
                        stream = await DrawGIFOverlay(pixels.AsBuffer());
                    else if (fullURL.Contains("youtube") || fullURL.Contains("youtu.be"))
                        stream = await DrawVideoOverlay(pixels.AsBuffer());
                    else
                        stream = await ReadImage(pixels.AsBuffer());

                    // and now save
                    file = await ImageCacheFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                    using (var fileStream = await file.OpenStreamForWriteAsync())
                    using (var saveStream = stream.AsStream())
                    {
                        saveStream.Position = 0;
                        await saveStream.CopyToAsync(fileStream);
                    }

                    return new Uri(string.Format("ms-appdata:///temp/ImageCache/{0}", fileName));
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error downloading image.", e);
                return null;
            }
        }

        public async Task RemoveCachedImage(string previewURL)
        {
            var filename = GetFilename(previewURL);

            try
            {
                var file = await ImageCacheFolder.GetFileAsync(filename);
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch(Exception e)
            {
                Logger.Error("Error deleting image.", e);
            }
        }
        #endregion

        #region Hashtag suggestions
        private ObservableCollectionEx<string> _hashtagSuggestions = null;
        public ObservableCollectionEx<string> HashtagSuggestions
        {
            get { return _hashtagSuggestions ?? (_hashtagSuggestions = new ObservableCollectionEx<string>()); }
        }

        public void GenerateSuggestions(string input, bool hashtag)
        {
            HashtagSuggestions.Clear();

            if (hashtag)
            {
                var sugs = ObservedHashtags.Where(x => x.StartsWith(input));
                HashtagSuggestions.AddRange(sugs);
                sugs = PopularHashtags.Where(x => x.StartsWith(input));
                HashtagSuggestions.AddRange(sugs);
            }
            else
            {
                var sugs = TempUsers.Where(x => x.StartsWith(input, StringComparison.CurrentCultureIgnoreCase));
                HashtagSuggestions.AddRange(sugs);
                sugs = ObservedUsers.Where(x => x.StartsWith(input, StringComparison.CurrentCultureIgnoreCase));
                HashtagSuggestions.AddRange(sugs);
            }
        }
        #endregion
    }
}
