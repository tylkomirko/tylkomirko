using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Mirko.Utils;
using Newtonsoft.Json;
using WykopSDK.API.Models;

namespace Mirko.ViewModel
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
                success = await App.ApiService.UnobserveUser(user);
            else
                success = await App.ApiService.ObserveUser(user);

            if(success)
            {
                string text = null;
                if(observed)
                {
                    text = "Przestałeś obserwować @" + user + ".";
                    Messenger.Default.Send(new NotificationMessage<string>(user, "Remove ObservedUser"));
                }
                else
                {
                    text = "Obserwujesz @" + user;
                    Messenger.Default.Send(new NotificationMessage<string>(user, "Add ObservedUser"));
                }

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

        private void ExecuteBlacklist()
        {
            bool blacklisted = (bool)Data.Blacklisted;
            var user = "@" + Data.Login;

            var blacklistVM = SimpleIoc.Default.GetInstance<BlacklistViewModel>();

            if (blacklisted)
                blacklistVM.UnblockPerson.Execute(user);
            else
                blacklistVM.BlockPerson.Execute(user);

            DispatcherHelper.CheckBeginInvokeOnUI(() => Data.Blacklisted = !blacklisted);
        }

        private RelayCommand _pm = null;
        [JsonIgnore]
        public RelayCommand PM
        {
            get { return _pm ?? (_pm = new RelayCommand(ExecutePM)); }
        }

        private void ExecutePM()
        {
            Messenger.Default.Send(new NotificationMessage<Profile>(Data, "Go to"));
        }
        
    }
}
