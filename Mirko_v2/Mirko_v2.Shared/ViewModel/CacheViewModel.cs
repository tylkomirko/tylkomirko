using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using System.Linq;
using MetroLog;
using System.Threading;
using System.Threading.Tasks;
using WykopAPI.Models;
using System.IO;
using Newtonsoft.Json;

namespace Mirko_v2.ViewModel
{
    public class CacheViewModel : ViewModelBase
    {
        private readonly TimeSpan FileLifeSpan = new TimeSpan(24, 0, 0);
        private Timer SaveTimer = null;
        private readonly ILogger Logger = null;

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
        private List<string> _cachedImages = null;
        public List<string> CachedImages
        {
            get { return _cachedImages ?? (_cachedImages = new List<string>()); }
        }

        private ObservableCollectionEx<string> _popularHashtags = null;
        public ObservableCollectionEx<string> PopularHashtags
        {
            get { return _popularHashtags ?? (_popularHashtags = new ObservableCollectionEx<string>()); }
        }

        private List<string> _observedHashtags;
        public List<string> ObservedHashtags
        {
            get { return _observedHashtags ?? (_observedHashtags = new List<string>()); }
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

            // CachedImages
            try
            {
                var folder = await tempFolder.GetFolderAsync("ImageCache");
                var files = await folder.GetFilesAsync();
                Logger.Info(files.Count + " images in cache.");
                CachedImages.AddRange(files.Select(x => x.Name));
                files = null;
            }
            catch (Exception)
            {
            }

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
                PopularHashtags.Clear();
                PopularHashtags.AddRange(data.Select(x => x.HashtagName));
                data = null;
            }

            // ObservedTags
            needToDownload = false;
            try
            {
                var file = await tempFolder.GetFileAsync("ObservedTags");
                var props = await file.GetBasicPropertiesAsync();
                if (DateTime.Now - props.DateModified > FileLifeSpan)
                {
                    needToDownload = true;
                }
                else
                {
                    var fileContent = await FileIO.ReadLinesAsync(file);
                    Logger.Info(fileContent.Count + " entries in ObservedTags");
                    ObservedHashtags.Clear();
                    ObservedHashtags.AddRange(fileContent);
                }
            }
            catch (Exception)
            {
                needToDownload = true;
            }

            if (needToDownload)
            {
                Logger.Info("Downloading ObservedHashtags.");
                var data = await App.ApiService.getUserObservedTags();
                ObservedHashtags.Clear();
                ObservedHashtags.AddRange(data);
                data = null;
            }
        }

        private async Task Save()
        {
            var tempFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;

            // PopularTags
            var file = await tempFolder.CreateFileAsync("PopularTags", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteLinesAsync(file, PopularHashtags);
            Logger.Info("Saved PopularHashtags, " + PopularHashtags.Count + " entries.");

            // ObervedTags
            file = await tempFolder.CreateFileAsync("ObservedTags", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteLinesAsync(file, ObservedHashtags);
            Logger.Info("Saved ObservedHashtags, " + ObservedHashtags.Count + " entries.");
        }
        #endregion
    }
}
