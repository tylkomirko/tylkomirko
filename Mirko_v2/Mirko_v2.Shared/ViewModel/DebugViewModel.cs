using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MetroLog;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Mirko_v2.ViewModel
{
    public class DebugViewModel : ViewModelBase
    {
        private readonly ILogger Logger = null;

        private ObservableCollectionEx<string> _registeredBackgroundTasks;
        public ObservableCollectionEx<string> RegisteredBackgroundTasks
        {
            get { return _registeredBackgroundTasks ?? (_registeredBackgroundTasks = new ObservableCollectionEx<string>()); }
        }

        private string _pseudoPushLastTime = "nigdy";
        public string PseudoPushLastTime
        {
            get { return _pseudoPushLastTime; }
            set { Set(() => PseudoPushLastTime, ref _pseudoPushLastTime, value); }
        }

        private string _cleanerLastTime = "nigdy";
        public string CleanerLastTime
        {
            get { return _cleanerLastTime; }
            set { Set(() => CleanerLastTime, ref _cleanerLastTime, value); }
        }

        private int _imgCacheHits = 0;
        public int ImgCacheHits
        {
            get { return _imgCacheHits; }
            set { Set(() => ImgCacheHits, ref _imgCacheHits, value); }
        }

        private float _imgCacheSaved = 0;
        public float ImgCacheSaved
        {
            get { return _imgCacheSaved; }
            set { Set(() => ImgCacheSaved, ref _imgCacheSaved, value); }
        }

        private ObservableCollectionEx<string> _logs = null;
        public ObservableCollectionEx<string> Logs
        {
            get { return _logs ?? (_logs = new ObservableCollectionEx<string>()); }
        }

        public DebugViewModel()
        {
            Logger = LogManagerFactory.DefaultLogManager.GetLogger<DebugViewModel>();
            Messenger.Default.Register<NotificationMessage<ulong>>(this, ReadMessage);
        }

        private void ReadMessage(NotificationMessage<ulong> obj)
        {
            if (obj.Notification == "ImgCacheHit")
            {
                ImgCacheHits++;
                ImgCacheSaved += obj.Content / 1024;

                Logger.Info("Image cache hits: " + ImgCacheHits);
                Logger.Info("Bandwidth saved: " + ImgCacheSaved + " KB");
            }
        }

        private RelayCommand _updateCommand = null;
        public RelayCommand UpdateCommand
        {
            get { return _updateCommand ?? (_updateCommand = new RelayCommand(ExecuteUpdateCommand)); }
        }

        private void ExecuteUpdateCommand()
        {
            RegisteredBackgroundTasks.Clear();
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
                RegisteredBackgroundTasks.Add(cur.Value.Name);

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings.Values;

            if (localSettings.ContainsKey("PseudoPushLastTime"))
            {
                var binary = (long)localSettings["PseudoPushLastTime"];
                PseudoPushLastTime = DateTime.FromBinary(binary).ToString("G");
            }

            if (localSettings.ContainsKey("CleanerLastTime"))
            {
                var binary = (long)localSettings["CleanerLastTime"];
                CleanerLastTime = DateTime.FromBinary(binary).ToString("G");
            }
        }

        private RelayCommand _shareCommand = null;
        public RelayCommand ShareCommand
        {
            get { return _shareCommand ?? (_shareCommand = new RelayCommand(ExecuteShareCommand)); }
        }

        private async void ExecuteShareCommand()
        {
            var logFile = await GetCompressedLogFile();
            if (logFile == null)
                return;

            var emailMessage = new EmailMessage();

            var attachmentName = logFile.Name;
            var attachmentStream = RandomAccessStreamReference.CreateFromFile(logFile);
            var attachment = new EmailAttachment(attachmentName, attachmentStream);

            var emailRecipient = new EmailRecipient("TylkoMirko@hotmail.com", "Tylko Mirko Support");

            emailMessage.Attachments.Add(attachment);
            emailMessage.To.Add(emailRecipient);
            emailMessage.Subject = "Tylko Mirko (logi)";

            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

        private async Task<IStorageFile> GetCompressedLogFile()
        {
            var lm = (IWinRTLogManager)LogManagerFactory.DefaultLogManager;
            var stream = await lm.GetCompressedLogs();

            if (stream != null)
            {
                // create a temp file
                var file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
                    string.Format("Log - {0}.zip", DateTime.UtcNow.ToString("yyyy-MM-dd HHmmss", CultureInfo.InvariantCulture)), CreationCollisionOption.ReplaceExisting);

                using (var ras = (await file.OpenAsync(FileAccessMode.ReadWrite)).AsStreamForWrite())
                {
                    await stream.CopyToAsync(ras);
                }

                stream.Dispose();

                return file;
            }

            return null;
        }
    }
}
