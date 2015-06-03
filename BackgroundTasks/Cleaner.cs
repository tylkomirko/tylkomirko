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
            StorageFolder folder = null;

            try
            {
                folder = await localFolder.GetFolderAsync("ImageCache");

                if (folder != null)
                {
                    var properties = await folder.GetBasicPropertiesAsync();
                    ulong size = properties.Size;

                    var files = await folder.GetFilesAsync();
                    var sortedFiles = files.OrderBy(x => x.DateCreated);

                    int i = 0;
                    while(size >= ThresholdSize)
                    {
                        var file = sortedFiles.ElementAt(i);
                        var fileProps = await file.GetBasicPropertiesAsync();

                        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        size -= fileProps.Size;

                        i++;
                    }                    
                }
            }
            catch (Exception e)
            {                
                
            }
        }
    }
}
