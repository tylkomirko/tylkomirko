using MetroLog;
using MetroLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace BackgroundTasks
{
    public sealed class Cleaner : IBackgroundTask
    {
        private readonly ILogger Logger = null;
        private const ulong ThresholdSize = 2 * 1024 * 1024; // 2 MB

        public Cleaner()
        {
            var configuration = new LoggingConfiguration();
#if DEBUG
            configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new DebugTarget());
#endif
            configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new FileStreamingTarget() { RetainDays = 7 });
            configuration.IsEnabled = true;

            try
            {
                LogManagerFactory.DefaultConfiguration = configuration;
            }
            catch (Exception) { }

            Logger = LogManagerFactory.DefaultLogManager.GetLogger<Cleaner>();
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance != null ? taskInstance.GetDeferral() : null;
            var cts = new CancellationTokenSource();

            if (taskInstance != null)
            {
                taskInstance.Canceled += (s, e) =>
                {
                    cts.Cancel();
                    cts.Dispose();
                };
            }

            try
            {
                Logger.Trace("Image cache cleaner started.");
                await CleanImageCache(cts.Token);
                ApplicationData.Current.LocalSettings.Values["CleanerLastTime"] = DateTime.Now.ToBinary();
            }
            catch (Exception e)
            {
                Logger.Error("Cleaner failed: ", e);
            }
            finally
            {
                if(deferral != null)
                    deferral.Complete();
            }
        }

        private async Task CleanImageCache(CancellationToken ct)
        {
            var localFolder = ApplicationData.Current.TemporaryFolder;
            StorageFolder folder = await localFolder.GetFolderAsync("ImageCache");

            if (folder == null)
                return;

            var files = await folder.GetFilesAsync();
            var sortedFiles = files.OrderBy(x => x.DateCreated);
            var dic = new Dictionary<StorageFile, ulong>();

            foreach (var file in sortedFiles)
            {
                ct.ThrowIfCancellationRequested();

                var fileProps = await file.GetBasicPropertiesAsync();                
                dic.Add(file, fileProps.Size);
            }
            
            var totalSize = (ulong)dic.Values.Sum(l => (long)l);            

            int i = 0;
            while (totalSize >= ThresholdSize)
            {
                var file = sortedFiles.ElementAt(i);

                try
                {
                    ct.ThrowIfCancellationRequested();

                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    totalSize -= dic[file];
                }
                catch (UnauthorizedAccessException) { }
                catch (FileNotFoundException) { }
                catch (Exception ex) { throw ex; }

                i++;
            }

            Logger.Trace("Removed " + i + " images."); 
        }
    }
}
