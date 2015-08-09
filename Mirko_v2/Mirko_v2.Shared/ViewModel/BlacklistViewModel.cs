using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using Mirko_v2.Utils;
using System.Threading.Tasks;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;

namespace Mirko_v2.ViewModel
{
    public class BlacklistViewModel : ViewModelBase
    {
        private ObservableCollectionEx<string> _tags = null;
        public ObservableCollectionEx<string> Tags
        {
            get { return _tags ?? (_tags = new ObservableCollectionEx<string>()); }
        }

        private ObservableCollectionEx<string> _people = null;
        public ObservableCollectionEx<string> People
        {
            get { return _people ?? (_people = new ObservableCollectionEx<string>()); }
        }

        public BlacklistViewModel()
        {
            Messenger.Default.Register<NotificationMessage>(this, ReadMessage);
        }

        private async void ReadMessage(NotificationMessage obj)
        {
            if(obj.Notification == "Init" || obj.Notification == "Login")
            {
                Tuple<List<string>, List<string>> tuple = null;
                try
                {
                    tuple = await Task.Run(() => App.WWWService.GetBlacklists());
                }
                catch (Exception e)
                {
                    App.TelemetryClient.TrackException(e);
                }

                if (tuple != null)
                {
                    var tags = tuple.Item1;
                    var users = tuple.Item2;

                    Tags.Clear();
                    Tags.AddRange(tags.Select(x => "#" + x));

                    People.Clear();
                    People.AddRange(users.Select(x => "@" + x));
                }
            } 
            else if(obj.Notification == "Logout")
            {
                Tags.Clear();
                People.Clear();
            }
        }

        private RelayCommand<string> _blacklistTag = null;
        public RelayCommand<string> BlacklistTag
        {
            get { return _blacklistTag ?? (_blacklistTag = new RelayCommand<string>(ExecuteBlacklistTag)); }
        }

        private async void ExecuteBlacklistTag(string hashtag)
        {
            var success = await App.ApiService.blockTag(hashtag);
            if (success)
            {
                Tags.Add(hashtag);
                Tags.Sort();

                await WykopSDK.WykopSDK.LocalStorage.SaveBlacklists(Tags.Select(x => x.Substring(1)));

                await StatusBarManager.ShowTextAsync(hashtag + " został zablokowany.");
            }
            else
            {
                await StatusBarManager.ShowTextAsync("Nie udało się zablokować " + hashtag + ".");
            }
        }

        private RelayCommand<string> _unblacklistTag = null;
        public RelayCommand<string> UnblacklistTag
        {
            get { return _unblacklistTag ?? (_unblacklistTag = new RelayCommand<string>(ExecuteUnblacklistTag)); }
        }

        private async void ExecuteUnblacklistTag(string hashtag)
        {
            var success = await App.ApiService.unblockTag(hashtag);
            if (success)
            {
                Tags.Remove(hashtag);
                await WykopSDK.WykopSDK.LocalStorage.SaveBlacklists(Tags.Select(x => x.Substring(1)));

                await StatusBarManager.ShowTextAsync(hashtag + " został odblokowany.");
            }
            else
            {
                await StatusBarManager.ShowTextAsync("Nie udało się odblokować " + hashtag + ".");
            }
        }

        private RelayCommand<string> _blockPerson = null;
        public RelayCommand<string> BlockPerson
        {
            get { return _blockPerson ?? (_blockPerson = new RelayCommand<string>(ExecuteBlockPerson)); }
        }

        private async void ExecuteBlockPerson(string username)
        {
            var success = await App.ApiService.blockUser(username.Substring(1)); // skip '@'
            if (success)
            {
                People.Add(username);
                People.Sort();

                await WykopSDK.WykopSDK.LocalStorage.SaveBlacklists(null, People.Select(x => x.Substring(1)));

                await StatusBarManager.ShowTextAsync(username + " został zablokowany.");
            }
            else
            {
                await StatusBarManager.ShowTextAsync("Nie udało się zablokować " + username + ".");
            }
        }

        private RelayCommand<string> _unblockPerson = null;
        public RelayCommand<string> UnblockPerson
        {
            get { return _unblockPerson ?? (_unblockPerson = new RelayCommand<string>(ExecuteUnblockPerson)); }
        }

        private async void ExecuteUnblockPerson(string username)
        {
            var success = await App.ApiService.unblockUser(username.Substring(1)); // skip '@'
            if (success)
            {
                People.Remove(username);
                await WykopSDK.WykopSDK.LocalStorage.SaveBlacklists(null, People.Select(x => x.Substring(1)));

                await StatusBarManager.ShowTextAsync(username + " został odblokowany.");
            }
            else
            {
                await StatusBarManager.ShowTextAsync("Nie udało się odblokować " + username + ".");
            }
        }
    }
}
