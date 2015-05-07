using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using System.Linq;

namespace Mirko_v2.ViewModel
{
    public class NewEntryViewModel : ViewModelBase
    {
        private ObservableCollectionEx<StorageItemThumbnail> _thumbs = null;
        public ObservableCollectionEx<StorageItemThumbnail> Thumbs
        {
            get { return _thumbs ?? (_thumbs = new ObservableCollectionEx<StorageItemThumbnail>()); }
        }

        private RelayCommand _loadPictures = null;
        public RelayCommand LoadPictures
        {
            get { return _loadPictures ?? (_loadPictures = new RelayCommand(ExecuteLoadPictures)); }
        }

        private async void ExecuteLoadPictures()
        {
            Thumbs.Clear();
            var fileTypeFilter = new List<string>();
            fileTypeFilter.Add(".jpg");
            fileTypeFilter.Add(".jpeg");
            fileTypeFilter.Add(".png");
            fileTypeFilter.Add(".gif");

            var files = await KnownFolders.PicturesLibrary.GetFilesAsync(CommonFileQuery.OrderByDate);
            //var filteredFiles = files.Where(x => fileTypeFilter.Contains(x.));
            foreach(var file in files)
            {
                var thumb = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView);
                Thumbs.Add(thumb);
            }
        }
    }
}
