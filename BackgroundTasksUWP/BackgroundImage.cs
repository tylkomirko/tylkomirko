using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System.UserProfile;
using WykopSDK.API;
using WykopSDK.API.Models;

namespace BackgroundTasksUWP
{
    public sealed class BackgroundImage : IBackgroundTask
    {
        private WykopAPI ApiService = null;
        private Random Random = null;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            string hashtag = null;
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("BackgroundHashtag"))
                hashtag = (string)ApplicationData.Current.LocalSettings.Values["BackgroundHashtag"];

            bool setLockscreen = false;
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("SetLockscreen"))
                setLockscreen = (bool)ApplicationData.Current.LocalSettings.Values["SetLockscreen"];

            bool setWallpaper = false;
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("SetWallpaper"))
                setWallpaper = (bool)ApplicationData.Current.LocalSettings.Values["SetWallpaper"];

            if (string.IsNullOrEmpty(hashtag) || !UserProfilePersonalizationSettings.IsSupported())
            {
                deferral.Complete();
                return;
            }

            // everything is fine, let's get dem images
            ApiService = new WykopAPI();
            Random = new Random();

            var cts = new CancellationTokenSource();
            taskInstance.Canceled += (s, e) =>
            {
                cts.Cancel();
                cts.Dispose();
            };

            try
            {
                var file = await GetImage(hashtag, cts.Token);

                if(setWallpaper)
                    await UserProfilePersonalizationSettings.Current.TrySetWallpaperImageAsync(file);

                if(setLockscreen)
                    await UserProfilePersonalizationSettings.Current.TrySetLockScreenImageAsync(file);

                ApplicationData.Current.LocalSettings.Values["BackgroundImageLastTime"] = DateTime.Now.ToBinary();
            }
            catch (Exception)
            {

            }
            finally
            {
                ApiService.Dispose();
                deferral.Complete();
            }
        }

        private async Task<StorageFile> GetImage(string hashtag, CancellationToken ct)
        {
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Lockscreen", CreationCollisionOption.ReplaceExisting);
            StorageFile outFile = null;
            Entry selectedEntry = null;

            ct.ThrowIfCancellationRequested();
            var entries = await ApiService.GetTaggedEntries(hashtag, 0, ct).ConfigureAwait(false);
            var embedEntries = entries.Entries.Where(x => x.Embed != null).Where(x => !x.Embed.NSFW);

            if (embedEntries.Count() != 0)
                selectedEntry = embedEntries.ElementAt(Random.Next(0, embedEntries.Count() - 1));

            if (selectedEntry == null)
                return null;

            var url = selectedEntry.Embed.URL;
            var filename = Path.GetFileName(url);
            outFile = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            using (var inStream = await ApiService.HttpClient.GetStreamAsync(url).ConfigureAwait(false))
            using (var randStream = await outFile.OpenAsync(FileAccessMode.ReadWrite))
            using (var outStream = randStream.AsStream())
            {
                await inStream.CopyToAsync(outStream).ConfigureAwait(false);
            }

            return outFile;
        }
    }
}
