using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mirko.Utils;
using Newtonsoft.Json;
using System;
using System.IO;
using Windows.Storage;
using Windows.System;
using WykopSDK.API.Models;

namespace Mirko.ViewModel
{
    public class EmbedViewModel : ViewModelBase
    {
        public Embed EmbedData { get; set; }
        public bool ImageShown { get; set; }
        public bool ErrorShown { get; set; }

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
            SimpleIoc.Default.GetInstance<NavigationService>().NavigateTo("EmbedPage");
        }

        private RelayCommand _saveImageCommand = null;
        [JsonIgnore]
        public RelayCommand SaveImageCommand
        {
            get { return _saveImageCommand ?? (_saveImageCommand = new RelayCommand(ExecuteSaveImageCommand)); }
        }

        private async void ExecuteSaveImageCommand()
        {
            if (EmbedData == null) return;

            var folder = KnownFolders.SavedPictures;
            var fileName = Path.GetFileName(EmbedData.Source);

            try
            {
                var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                using (var stream = await App.ApiService.httpClient.GetStreamAsync(new Uri(EmbedData.URL)))
                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    await stream.CopyToAsync(fileStream);

                    StatusBarManager.ShowText("Zapisano obraz.");
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
            if (ErrorShown)
                return;

            var url = EmbedData.URL;
            if (url.EndsWith(".jpg") || url.EndsWith(".jpeg") || url.EndsWith(".png") || (url.Contains("imgwykop.pl") && !url.EndsWith("gif")))
            {
                StatusBarManager.ShowTextAndProgress("Pobieram obraz...");
                GoToEmbedPage();
            }
            else if (url.EndsWith(".gif"))
            {
                await StatusBarManager.ShowTextAndProgressAsync("Konwertuje GIF...");
                var mp4 = await Gfycat.Gfycat.GIFtoMP4(EmbedData.URL);

                if (!string.IsNullOrEmpty(mp4))
                    MediaElementSrc = mp4;
                else
                    StatusBarManager.ShowText("Coś poszło nie tak...");
            }
            else if (url.Contains("gfycat.com"))
            {
                await StatusBarManager.ShowTextAndProgressAsync("Otwieram GFY...");
                var mp4 = await Gfycat.Gfycat.GFYgetURL(url);

                if (!string.IsNullOrEmpty(mp4))
                    MediaElementSrc = mp4;
                else
                    StatusBarManager.ShowText("Coś poszło nie tak...");
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

                await Launcher.LaunchUriAsync(new Uri(uri));
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

                await Launcher.LaunchUriAsync(new Uri(uri));
            }
            else
            {
                await Launcher.LaunchUriAsync(new Uri(url));
            }
        }
    }
}
