using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using System.Linq;

namespace Mirko_v2.ViewModel
{
    public class CacheViewModel : ViewModelBase
    {
        private readonly TimeSpan FileLifeSpan = new TimeSpan(24, 0, 0);

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
                    PopularHashtags.Clear();
                    PopularHashtags.AddRange(fileContent);
                }
            } 
            catch(Exception)
            {
                needToDownload = true;
            }

            if (needToDownload)
            {
                var data = await App.ApiService.getPopularTags();
                PopularHashtags.Clear();
                PopularHashtags.AddRange(data.Select(x => x.HashtagName));
                data = null;
            }

            // ObservedTags
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
                var data = await App.ApiService.getUserObservedTags();
                ObservedHashtags.Clear();
                ObservedHashtags.AddRange(data);
                data = null;
            }
        }
        #endregion
    }
}
