using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace BackgroundTasks
{
    public sealed class Cleaner : IBackgroundTask
    {
        private const ulong ThresholdSize = 10 * 1024 * 1024;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            await CleanImageCache();

            deferral.Complete();
        }

        private async Task CleanImageCache()
        {
            var localFolder = ApplicationData.Current.TemporaryFolder;
            StorageFolder folder = await localFolder.GetFolderAsync("ImageCache");

            folder = await localFolder.GetFolderAsync("ImageCache");

            if (folder != null)
            {
                var files = await folder.GetFilesAsync();
                var fileSizeTasks = files.Select(async x => (await x.GetBasicPropertiesAsync()).Size);
                var sizes = await Task.WhenAll(fileSizeTasks);
                ulong size = (ulong)sizes.Sum(l => (long)l);

                var sortedFiles = files.OrderBy(x => x.DateCreated);

                int i = 0;
                while (size >= ThresholdSize)
                {
                    var file = sortedFiles.ElementAt(i);
                    var fileProps = await file.GetBasicPropertiesAsync();

                    try
                    {
                        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        size -= fileProps.Size;
                    }
                    catch (Exception) { }

                    i++;
                }
            }
        }
    }
}
