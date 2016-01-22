using System;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace BackgroundTasks
{
    public sealed class UpdateTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            var tempFolder = ApplicationData.Current.TemporaryFolder;
            var localFolder = ApplicationData.Current.LocalFolder;

            try
            {
                var imgCacheFolder = await tempFolder.GetFolderAsync("ImageCache");
                await imgCacheFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
            } 
            catch(Exception) { }

            try
            {
                var imgCacheFolder = await localFolder.GetFolderAsync("ImageService");
                await imgCacheFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception) { }

            deferral.Complete();
        }
    }
}
