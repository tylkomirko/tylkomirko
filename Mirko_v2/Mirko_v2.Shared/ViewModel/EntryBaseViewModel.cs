﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mirko_v2.Common;
using Mirko_v2.Utils;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using WykopAPI.Models;
using System.Threading.Tasks;
using System;

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

                if(verbose)
                    StatusBarManager.ShowText(DataBase.Voted ? "Dodano plusa." : "Cofnięto plusa.");
            }
            else
            {
                if(verbose)
                    StatusBarManager.ShowText("Nie udało się oddać głosu.");
            }
        }

        private RelayCommand _deleteCommand = null;
        [JsonIgnore]
        public RelayCommand DeleteCommand
        {
            get { return _deleteCommand ?? (_deleteCommand = new RelayCommand(ExecuteDeleteCommand)); }
        }

        private void ExecuteDeleteCommand()
        {
            throw new System.NotImplementedException();
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
