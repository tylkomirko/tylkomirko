using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mirko_v2.Common;
using Mirko_v2.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using WykopSDK.API.Models;

namespace Mirko_v2.ViewModel
{
    public class EntryBaseViewModel : ViewModelBase
    {
        public EntryBase DataBase { get; set; }
        public EmbedViewModel EmbedVM { get; set; }

        private string _tappedHashtag = null;
        [JsonIgnore]
        public string TappedHashtag
        {
            get { return _tappedHashtag; }
            set { Set(() => TappedHashtag, ref _tappedHashtag, value); }
        }

        private bool _showVoters = false;
        [JsonIgnore]
        public bool ShowVoters
        {
            get { return _showVoters; }
            set { Set(() => ShowVoters, ref _showVoters, value); }
        }

        public EntryBaseViewModel()
        {
        }

        public EntryBaseViewModel(EntryBase d)
        {
            DataBase = d;
            if(d.Embed != null)
                EmbedVM = new EmbedViewModel(DataBase.Embed);
            DataBase.Embed = null;
        }

        private RelayCommand _voteCommand = null;
        [JsonIgnore]
        public RelayCommand VoteCommand
        {
            get { return _voteCommand ?? (_voteCommand = new RelayCommand(async () => await ExecuteVoteCommand())); }
        }

        private async Task ExecuteVoteCommand(bool verbose = true)
        {
            if(App.ApiService.UserInfo == null)
            {
                StatusBarManager.ShowText("Musisz być zalogowany.");
                return;
            }

            if(verbose)
                StatusBarManager.ShowProgress();

            Vote reply = null;
            if (DataBase is EntryComment)
            {
                var c = DataBase as EntryComment;
                reply = await App.ApiService.voteEntry(id: c.EntryID, commentID: c.ID, upVote: !c.Voted, isItEntry: false);
            }
            else
            {
                reply = await App.ApiService.voteEntry(id: DataBase.ID, upVote: !DataBase.Voted);
            }

            if (reply != null)
            {
                DataBase.VoteCount = reply.VoteCount;
                DataBase.Voted = !DataBase.Voted;
                DataBase.Voters = reply.Voters;

                App.TelemetryClient.TrackEvent(DataBase.Voted ? "Upvote" : "Downvote");

                if (verbose)
                    StatusBarManager.ShowText(DataBase.Voted ? "Dodano plusa." : "Cofnięto plusa.");
            }
            else
            {
                if(verbose)
                    StatusBarManager.ShowText("Nie udało się oddać głosu.");
            }
        }

        private RelayCommand _replyCommand = null;
        [JsonIgnore]
        public RelayCommand ReplyCommand
        {
            get { return _replyCommand ?? (_replyCommand = new RelayCommand(ExecuteReplyCommand)); }
        }

        private void ExecuteReplyCommand()
        {
            var VM = SimpleIoc.Default.GetInstance<NewEntryViewModel>();

            if(DataBase is EntryComment)
            {
                var d = DataBase as EntryComment;
                VM.NewEntry.EntryID = d.EntryID;
            }
            else
            {
                VM.NewEntry.EntryID = DataBase.ID;
            }
            
            VM.NewEntry.CommentID = 0;
            VM.NewEntry.IsEditing = false;
            VM.GoToNewEntryPage(new List<EntryBaseViewModel>() { this });
        }

        private RelayCommand _deleteCommand = null;
        [JsonIgnore]
        public RelayCommand DeleteCommand
        {
            get { return _deleteCommand ?? (_deleteCommand = new RelayCommand(ExecuteDeleteCommand)); }
        }

        private async void ExecuteDeleteCommand()
        {
            if (DataBase is EntryComment)
            {
                var d = DataBase as EntryComment;
                var result = await App.ApiService.deleteEntry(rootID: d.EntryID, id: d.ID, isComment: true);
                if(result == d.ID)
                {
                    StatusBarManager.ShowText("Komentarz został usunięty.");
                    Messenger.Default.Send<Tuple<uint, uint>>(new Tuple<uint, uint>(d.EntryID, d.ID), "Remove comment");
                }
            }
            else
            {
                var result = await App.ApiService.deleteEntry(id: DataBase.ID);
                if(result == DataBase.ID)
                {
                    StatusBarManager.ShowText("Wpis został usunięty.");
                    Messenger.Default.Send<uint>(result, "Remove entry");
                }
            }
        }

        private RelayCommand _editCommand = null;
        [JsonIgnore]
        public RelayCommand EditCommand
        {
            get { return _editCommand ?? (_editCommand = new RelayCommand(ExecuteEditCommand)); }
        }

        private void ExecuteEditCommand()
        {
            var VM = SimpleIoc.Default.GetInstance<NewEntryViewModel>();

            if (DataBase is EntryComment)
            {
                var d = DataBase as EntryComment;
                VM.NewEntry.EntryID = d.EntryID;
                VM.NewEntry.CommentID = d.ID;
            }
            else
            {
                VM.NewEntry.EntryID = DataBase.ID;
            }

            VM.NewEntry.IsEditing = true;
            VM.GoToNewEntryPage(new List<EntryBaseViewModel>() { this });
        }

        private RelayCommand _refreshCommand = null;
        [JsonIgnore]
        public RelayCommand RefreshCommand
        {
            get { return _refreshCommand ?? (_refreshCommand = new RelayCommand(ExecuteRefreshCommand)); }
        }

        private async void ExecuteRefreshCommand()
        {
            if (DataBase == null) return;

            await StatusBarManager.ShowTextAndProgressAsync("Pobieram wpis...");
            var newEntry = await App.ApiService.getEntry(DataBase.ID);
            if (newEntry == null)
            {
                await StatusBarManager.ShowTextAsync("Nie udało się pobrać wpisu.");
            }
            else
            {
                var newVM = new EntryViewModel(newEntry);
                Messenger.Default.Send<EntryViewModel>(newVM, "Update");

                await StatusBarManager.HideProgressAsync();
            }
        }

        private RelayCommand<List<EntryBaseViewModel>> _voteMultiple = null;
        [JsonIgnore]
        public RelayCommand<List<EntryBaseViewModel>> VoteMultiple
        {
            get { return _voteMultiple ?? (_voteMultiple = new RelayCommand<List<EntryBaseViewModel>>(ExecuteVoteMultiple)); }
        }

        private async void ExecuteVoteMultiple(List<EntryBaseViewModel> list)
        {
            var onlyUpVotes = list.Where(x => !x.DataBase.Voted);
            if (onlyUpVotes.Count() == 0)
            {
                await StatusBarManager.ShowTextAsync("Plusa możesz dać tylko raz ( ͡° ͜ʖ ͡°)");
                return;
            }

            double progress = 0;
            double progressStep = 1 / (double)onlyUpVotes.Count();

            foreach(var entry in onlyUpVotes)
            {
                await StatusBarManager.ShowProgressAsync(progress);
                await entry.ExecuteVoteCommand(false);
                progress += progressStep;
            }

            await StatusBarManager.ShowTextAsync("Plusy zostały przyznane.");
        }

        #region Hashtag
        public void PrepareHashtagFlyout(ref MenuFlyout mf, string tag)
        {
            TappedHashtag = tag;

            if (App.ApiService.UserInfo == null)
            {
                MenuFlyoutUtils.MakeItemInvisible(ref mf, "observeTag");
                MenuFlyoutUtils.MakeItemInvisible(ref mf, "unobserveTag");
                MenuFlyoutUtils.MakeItemInvisible(ref mf, "blacklistTag");
            }
            else
            {
                var observedTags = SimpleIoc.Default.GetInstance<CacheViewModel>().ObservedHashtags;
                if (observedTags.Contains(tag))
                {
                    MenuFlyoutUtils.MakeItemInvisible(ref mf, "observeTag");
                    MenuFlyoutUtils.MakeItemVisible(ref mf, "unobserveTag");
                }
                else
                {
                    MenuFlyoutUtils.MakeItemVisible(ref mf, "observeTag");
                    MenuFlyoutUtils.MakeItemInvisible(ref mf, "unobserveTag");
                }
            }

            /*
            if (App.MainViewModel.BlacklistedTags.Contains(tag))
            {
                MenuFlyoutUtils.MakeItemInvisible(ref mf, "blacklistTag");
                MenuFlyoutUtils.MakeItemVisible(ref mf, "unblacklistTag");
            }
            else
            {
                MenuFlyoutUtils.MakeItemVisible(ref mf, "blacklistTag");
                MenuFlyoutUtils.MakeItemInvisible(ref mf, "unblacklistTag");
            }*/
        }

        private RelayCommand _blacklistHashtag = null;
        [JsonIgnore]
        public RelayCommand BlacklistHashtag
        {
            get { return _blacklistHashtag ?? (_blacklistHashtag = new RelayCommand(ExecuteBlacklistHashtag)); }
        }

        private async void ExecuteBlacklistHashtag()
        {
            var success = await App.ApiService.blockTag(TappedHashtag);
            if (success)
            {
                await StatusBarManager.ShowTextAsync("Tag " + TappedHashtag + " został zablokowany.");
            }
            else
            {
                await StatusBarManager.ShowTextAsync("Nie udało się zablokować tagu " + TappedHashtag + ".");
            }
        }
        #endregion

        #region Profile
        public void GoToProfilePage(string username)
        {
            var profilesVM = SimpleIoc.Default.GetInstance<ProfilesViewModel>();
            profilesVM.GoToProfile.Execute(username);
        }
        #endregion
    }
}
