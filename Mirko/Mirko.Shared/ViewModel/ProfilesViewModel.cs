using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Mirko.Utils;
using System.IO;
using Newtonsoft.Json;
using MetroLog;
using System.Threading.Tasks;

namespace Mirko.ViewModel
{
    public class ProfilesViewModel : ViewModelBase, IResumable
    {
        private readonly ILogger Logger = null;
        private NavigationService NavService = null;

        public ProfilesViewModel(NavigationService nav)
        {
            NavService = nav;
            Logger = LogManagerFactory.DefaultLogManager.GetLogger<ProfilesViewModel>();
        }

        private ProfileViewModel _currentProfile = null;
        public ProfileViewModel CurrentProfile
        {
            get { return _currentProfile; }
            set { Set(() => CurrentProfile, ref _currentProfile, value); }
        }

        private Dictionary<string, ProfileViewModel> _profiles = null;
        public Dictionary<string, ProfileViewModel> Profiles
        {
            get { return _profiles ?? (_profiles = new Dictionary<string, ProfileViewModel>()); }
        }

        private RelayCommand<string> _goToProfile = null;
        public RelayCommand<string> GoToProfile 
        {
            get { return _goToProfile ?? (_goToProfile = new RelayCommand<string>(ExecuteGoToProfile)); }
        }

        private async void ExecuteGoToProfile(string username)
        {
            if (string.IsNullOrEmpty(username)) return;

            if (Profiles.ContainsKey(username))
            {
                CurrentProfile = Profiles[username];
            }
            else
            {
                // download profile info
                await StatusBarManager.ShowTextAndProgressAsync("Pobieram profil...");
                var profile = await App.ApiService.getProfile(username);
                if(profile != null)
                {
                    await StatusBarManager.HideProgressAsync();
                    var profileVM = new ProfileViewModel(profile);
                    Profiles[username] = profileVM;
                    CurrentProfile = profileVM;
                }
                else
                {
                    await StatusBarManager.ShowTextAsync("Nie udało pobrać się profilu.");
                    return;
                }
            }

            NavService.NavigateTo("ProfilePage");
        }

        #region IResumable
        public async Task SaveState(string pageName)
        {
            var folder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("VMs", Windows.Storage.CreationCollisionOption.OpenIfExists);
            var file = await folder.CreateFileAsync("ProfilesViewModel", Windows.Storage.CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenStreamForWriteAsync())
            using (var sw = new StreamWriter(stream))
            using (var writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.None;
                JsonSerializer serializer = new JsonSerializer();

                serializer.Serialize(writer, CurrentProfile);
            }
        }

        public async Task<bool> LoadState(string pageName)
        {
            var folder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync("VMs");
            var file = await folder.GetFileAsync("ProfilesViewModel");

            using (var stream = await file.OpenStreamForReadAsync())
            using (var sr = new StreamReader(stream))
            using (var reader = new JsonTextReader(sr))
            {
                JsonSerializer serializer = new JsonSerializer();
                var profileVM = serializer.Deserialize<ProfileViewModel>(reader);

                CurrentProfile = profileVM;
                Profiles[profileVM.Data.Login] = profileVM;
            }

            return true; // success!
        }

        public string GetName()
        {
            return "ProfilesViewModel";
        }
        #endregion
    }
}
