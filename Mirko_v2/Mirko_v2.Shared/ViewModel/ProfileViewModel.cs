using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using Mirko_v2.Utils;
using Newtonsoft.Json;
using WykopSDK.API.Models;

namespace Mirko_v2.ViewModel
{
    public class ProfileViewModel : ViewModelBase
    {
        private Profile _data = null;
        public Profile Data
        {
            get { return _data; }
            set { Set(() => Data, ref _data, value); }
        }

        private IncrementalLoadingCollection<UserEntrySource, EntryViewModel> _entries = null;
        public IncrementalLoadingCollection<UserEntrySource, EntryViewModel> Entries
        {
            get { return _entries ?? (_entries = new IncrementalLoadingCollection<UserEntrySource, EntryViewModel>()); }
        }

        public ProfileViewModel()
        {
        }

        public ProfileViewModel(Profile d)
        {
            Data = d;
            d = null;
        }

        private RelayCommand _observe = null;
        [JsonIgnore]
        public RelayCommand Observe
        {
            get { return _observe ?? (_observe = new RelayCommand(ExecuteObserve)); }
        }

        private async void ExecuteObserve()
        {
            bool observed = (bool)Data.Observed;
            var user = Data.Login;
            bool success;

            await StatusBarManager.ShowProgressAsync();
            if (observed)
                success = await App.ApiService.unobserveUser(user);
            else
                success = await App.ApiService.observeUser(user);

            if(success)
            {
                string text = observed ? "Przestałeś obserwować @" + user : "Obserwujesz @" + user;
                DispatcherHelper.CheckBeginInvokeOnUI(() => Data.Observed = !observed);
                await StatusBarManager.ShowTextAsync(text);
            }
            else
            {
                await StatusBarManager.ShowTextAsync("Coś poszło nie tak...");
            }
        }

        private RelayCommand _blacklist = null;
        [JsonIgnore]
        public RelayCommand Blacklist
        {
            get { return _blacklist ?? (_blacklist = new RelayCommand(ExecuteBlacklist)); }
        }

        private async void ExecuteBlacklist()
        {
            bool blacklisted = (bool)Data.Blacklisted;
            var user = Data.Login;
            bool success;

            await StatusBarManager.ShowProgressAsync();
            if (blacklisted)
                success = await App.ApiService.unblockUser(user);
            else
                success = await App.ApiService.blockUser(user);

            if (success)
            {
                string text = blacklisted ? "Odblokowałeś @" + user : "Zablokowałeś @" + user;
                DispatcherHelper.CheckBeginInvokeOnUI(() => Data.Blacklisted = !blacklisted);
                await StatusBarManager.ShowTextAsync(text);
            }
            else
            {
                await StatusBarManager.ShowTextAsync("Coś poszło nie tak...");
            }
        }

        private RelayCommand _pm = null;
        [JsonIgnore]
        public RelayCommand PM
        {
            get { return _pm ?? (_pm = new RelayCommand(ExecutePM)); }
        }

        private void ExecutePM()
        {
            Messenger.Default.Send<NotificationMessage<Profile>>(new NotificationMessage<Profile>(Data, "Go to"));
        }
        
    }
}
