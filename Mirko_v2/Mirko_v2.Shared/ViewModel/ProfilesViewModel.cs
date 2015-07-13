using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Mirko_v2.Utils;
using System.IO;
using Newtonsoft.Json;
using MetroLog;

namespace Mirko_v2.ViewModel
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
                    await StatusBarManager.ShowText("Nie udało pobrać się profilu.");
                    return;
                }
            }

            NavService.NavigateTo("ProfilePage");
        }

        #region IResumable
        public async System.Threading.Tasks.Task SaveState(string pageName)
        {
            try
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
            catch (Exception e)
            {
                Logger.Error("Error saving state: ", e);
            }
        }

        public async System.Threading.Tasks.Task<bool> LoadState(string pageName)
        {
            try
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
            catch (Exception e)
            {
                Logger.Error("Error loading state: ", e);
                return false;
            }
        }

        public string GetName()
        {
            return "ProfilesViewModel";
        }
        #endregion
    }
}
