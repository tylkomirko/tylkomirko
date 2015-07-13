using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using Mirko_v2.Utils;
using Newtonsoft.Json;
using System;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using WykopAPI.Models;

namespace Mirko_v2.ViewModel
{
    public class EmbedViewModel : ViewModelBase
    {
        public Embed EmbedData { get; set; }
        public bool ImageShown { get; set; }

        private MemoryStream _imageStream = null;
        [JsonIgnore]
        public MemoryStream ImageStream
        {
            get { return _imageStream; }
            set
            {
                if (_imageStream != value)
                {
                    if (_imageStream != null)
                        _imageStream.Dispose();

                    _imageStream = value;
                }
            }
        }

        private BitmapImage _bitmap = null;
        [JsonIgnore]
        public BitmapImage Bitmap
        {
            get { return _bitmap; }
            set { Set(() => Bitmap, ref _bitmap, value); }
        }

        private string _mediaElementSrc = null;
        public string MediaElementSrc
        {
            get { return _mediaElementSrc; }
            set { Set(() => MediaElementSrc, ref _mediaElementSrc, value); }
        }

        public EmbedViewModel()
        {

        }

        public EmbedViewModel(Embed e)
        {
            EmbedData = e;
            e = null;
        }

        public void GoToEmbedPage()
        {
            Messenger.Default.Send<EmbedViewModel>(this, "Embed UserControl");
            SimpleIoc.Default.GetInstance<INavigationService>().NavigateTo("EmbedPage");
        }

        private RelayCommand _loadImageCommand = null;
        [JsonIgnore]
        public RelayCommand LoadImageCommand
        {
            get { return _loadImageCommand ?? (_loadImageCommand = new RelayCommand(ExecuteLoadImageCommand)); }
        }

        private async void ExecuteLoadImageCommand()
        {
            using (var stream = await App.ApiService.httpClient.GetStreamAsync(new Uri(EmbedData.URL)))
            {
                ImageStream = new MemoryStream();
                await stream.CopyToAsync(ImageStream);
                ImageStream.Position = 0;

                var sr = ImageStream.AsRandomAccessStream();
                Bitmap = new BitmapImage();
                Bitmap.SetSource(sr);
            }
        }

        private RelayCommand _saveImageCommand = null;
        [JsonIgnore]
        public RelayCommand SaveImageCommand
        {
            get { return _saveImageCommand ?? (_saveImageCommand = new RelayCommand(ExecuteSaveImageCommand)); }
        }

        private async void ExecuteSaveImageCommand()
        {
            if (ImageStream == null || ImageStream.Length == 0) return;

            var folder = KnownFolders.SavedPictures;
            var fileName = Path.GetFileName(EmbedData.Source);

            try
            {
                var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    fileStream.Seek(0, SeekOrigin.Begin);
                    ImageStream.Seek(0, SeekOrigin.Begin);
                    await ImageStream.CopyToAsync(fileStream);

                    await StatusBarManager.ShowText("Zapisano obraz.");
                }
            }
            catch (Exception) { }
        }

        private RelayCommand _openEmbedCommand = null;
        [JsonIgnore]
        public RelayCommand OpenEmbedCommand
        {
            get { return _openEmbedCommand ?? (_openEmbedCommand = new RelayCommand(ExecuteOpenEmbedCommand)); }
        }

        private async void ExecuteOpenEmbedCommand()
        {
            var url = EmbedData.URL;
            if (url.EndsWith(".jpg") || url.EndsWith(".jpeg") || url.EndsWith(".png") || (url.Contains("imgwykop.pl") && !url.EndsWith("gif")))
            {
                GoToEmbedPage();
            }
            else if (url.EndsWith(".gif"))
            {
                await StatusBarManager.ShowTextAndProgressAsync("Konwertuje GIF...");
                var mp4 = await Gfycat.Gfycat.GIFtoMP4(EmbedData.URL);

                if (!string.IsNullOrEmpty(mp4))
                    MediaElementSrc = mp4;
                else
                    await StatusBarManager.ShowText("Coś poszło nie tak...");
            }
            else if (url.Contains("gfycat.com"))
            {
                await StatusBarManager.ShowTextAndProgressAsync("Otwieram GFY...");
                var mp4 = await Gfycat.Gfycat.GFYgetURL(url);

                if (!string.IsNullOrEmpty(mp4))
                    MediaElementSrc = mp4;
                else
                    await StatusBarManager.ShowText("Coś poszło nie tak...");
            }
            else if (url.Contains("youtube")) // FIXME
            {
                string uri = "";
                var settingsVM = SimpleIoc.Default.GetInstance<SettingsViewModel>();

                if (settingsVM.SelectedYouTubeApp != YouTubeApp.IE)
                {
                    var index = url.IndexOf("watch?v=") + 8;
                    var id = url.Substring(index, 11);

                    if (settingsVM.SelectedYouTubeApp == YouTubeApp.TUBECAST)
                        uri = "tubecast:VideoID=" + id;
                    else if (settingsVM.SelectedYouTubeApp == YouTubeApp.METROTUBE)
                        uri = "metrotube:VideoPage?VideoID=" + id;
                    else if (settingsVM.SelectedYouTubeApp == YouTubeApp.TOIB)
                        uri = "toib:PlayVideo?VideoID=" + id;
                    else if (settingsVM.SelectedYouTubeApp == YouTubeApp.MYTUBE)
                        uri = "mytube:link=www.youtube.com/watch?v=" + id;
                }
                else
                {
                    uri = url;
                }

                await Windows.System.Launcher.LaunchUriAsync(new Uri(uri));
            }
            else if (url.Contains("youtu.be"))
            {
                string uri = "";
                var settingsVM = SimpleIoc.Default.GetInstance<SettingsViewModel>();

                if (settingsVM.SelectedYouTubeApp != YouTubeApp.IE)
                {
                    var index = url.IndexOf(".be/") + 4;
                    var id = url.Substring(index, 11);

                    if (settingsVM.SelectedYouTubeApp == YouTubeApp.TUBECAST)
                        uri = "tubecast:VideoID=" + id;
                    else if (settingsVM.SelectedYouTubeApp == YouTubeApp.METROTUBE)
                        uri = "metrotube:VideoPage?VideoID=" + id;
                    else if (settingsVM.SelectedYouTubeApp == YouTubeApp.TOIB)
                        uri = "toib:PlayVideo?VideoID=" + id;
                    else if (settingsVM.SelectedYouTubeApp == YouTubeApp.MYTUBE)
                        uri = "mytube:link=www.youtube.com/watch?v=" + id;
                }
                else
                {
                    uri = url;
                }

                await Windows.System.Launcher.LaunchUriAsync(new Uri(uri));
            }
            else
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
            }
        }
    }
}
