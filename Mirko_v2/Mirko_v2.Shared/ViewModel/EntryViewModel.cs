using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using Mirko_v2.Common;
using Mirko_v2.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;
using WykopAPI.Models;

namespace Mirko_v2.ViewModel
{
    public class EntryViewModel : ViewModelBase
    {
        public Entry Data { get; set; }
        public ObservableCollectionEx<CommentViewModel> Comments { get; set; }
        public EmbedViewModel EmbedVM { get; set; }

        private string _tappedHashtag = null;
        public string TappedHashtag
        {
            get { return _tappedHashtag; }
            set { Set(() => TappedHashtag, ref _tappedHashtag, value); }
        }
        
        public EntryViewModel()
        {

        }

        public EntryViewModel(Entry d)
        {
            Data = d;
            EmbedVM = new EmbedViewModel(Data.Embed);

            if (Data.Comments != null)
            {
                Comments = new ObservableCollectionEx<CommentViewModel>();
                foreach (var com in Data.Comments)
                    Comments.Add(new CommentViewModel(com));

                Data.Comments = null;
            }

            Data.Embed = null;
            d = null;
        }

        public void GoToEntryPage(bool isHot)
        {
            if (isHot)
            {
                Messenger.Default.Send<EntryViewModel>(this, "Hot Entry UserControl");
                if(Comments.Count == 0)
                    GetComments.Execute(null);
            }
            else
            {
                Messenger.Default.Send<EntryViewModel>(this, "Entry UserControl");
            }
            SimpleIoc.Default.GetInstance<INavigationService>().NavigateTo("EntryPage");
        }

        private RelayCommand _voteCommand = null;
        [JsonIgnore]
        public RelayCommand VoteCommand
        {
            get { return _voteCommand ?? (_voteCommand = new RelayCommand(ExecuteVoteCommand));  }
        }

        private async void ExecuteVoteCommand()
        {
            await StatusBarManager.ShowProgress();
            var reply = await App.ApiService.voteEntry(id: Data.ID, upVote: !Data.Voted);
            if(reply != null)
            {
                Data.VoteCount = (uint)reply.vote;
                Data.Voted = !Data.Voted;
                Data.Voters = reply.Voters;

                await StatusBarManager.ShowText(Data.Voted ? "Dodano plusa." : "Cofnięto plusa.");
            }
            else
            {
                await StatusBarManager.ShowText("Nie udało się oddać głosu.");
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
            throw new System.NotImplementedException();
        }

        private RelayCommand _favouriteCommand = null;
        [JsonIgnore]
        public RelayCommand FavouriteCommand
        {
            get { return _favouriteCommand ?? (_replyCommand = new RelayCommand(ExecuteFavouriteCommand)); }
        }

        private async void ExecuteFavouriteCommand()
        {
            var reply = await App.ApiService.addToFavourites(Data.ID);
            Data.Favourite = reply.user_favorite;
        }

        private RelayCommand _editCommand = null;
        [JsonIgnore]
        public RelayCommand EditCommand
        {
            get { return _editCommand ?? (_replyCommand = new RelayCommand(ExecuteEditCommand)); }
        }

        private void ExecuteEditCommand()
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
            if (Data == null) return;

            await StatusBarManager.ShowTextAndProgress("Pobieram wpis...");
            var newEntry = await App.ApiService.getEntry(Data.ID);
            if (newEntry == null)
            {
                await StatusBarManager.ShowText("Nie udało się pobrać wpisu.");
            }
            else
            {
                var newVM = new EntryViewModel(newEntry);
                Messenger.Default.Send<EntryViewModel>(newVM, "Update");

                await StatusBarManager.HideProgress();
            }
        }

        private RelayCommand _deleteCommand = null;
        [JsonIgnore]
        public RelayCommand DeleteCommand
        {
            get { return _deleteCommand ?? (_replyCommand = new RelayCommand(ExecuteDeleteCommand)); }
        }

        private void ExecuteDeleteCommand()
        {
            throw new System.NotImplementedException();
        }

        private RelayCommand _getComments = null;
        [JsonIgnore]
        public RelayCommand GetComments
        {
            get { return _getComments ?? (_getComments = new RelayCommand(ExecuteGetComments)); }
        }

        private async void ExecuteGetComments()
        {
            await StatusBarManager.ShowTextAndProgress("Pobieram komentarze...");

            var entry = await App.ApiService.getEntry(Data.ID);
            if(entry != null)
            {
                var comments = new List<CommentViewModel>(entry.Comments.Count);
                foreach(var c in entry.Comments)
                    comments.Add(new CommentViewModel(c));

                Comments.Clear();
                Comments.AddRange(comments);
                Data.CommentCount = entry.CommentCount;
                Data.VoteCount = entry.VoteCount;
                Data.Voters.Clear();
                Data.Voters.AddRange(entry.Voters);

                comments = null;
            }

            await StatusBarManager.HideProgress();
        }

        #region Hashtag
        public void PrepareHashtagFlyout(ref MenuFlyout mf, string tag)
        {
            TappedHashtag = tag;

            var observedTags = SimpleIoc.Default.GetInstance<NotificationsViewModel>().ObservedHashtags;
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

        private RelayCommand _observeHashtag = null;
        [JsonIgnore]
        public RelayCommand ObserveHashtag
        {
            get { return _observeHashtag ?? (_observeHashtag = new RelayCommand(ExecuteObserveHashtag)); }
        }

        private async void ExecuteObserveHashtag()
        {
            var observedTags = SimpleIoc.Default.GetInstance<NotificationsViewModel>().ObservedHashtags;

            await StatusBarManager.ShowProgress();
            if(observedTags.Contains(TappedHashtag))
            {
                var success = await App.ApiService.unobserveTag(TappedHashtag);
                if (success)
                {
                    await StatusBarManager.ShowText("Tag " + TappedHashtag + " został usunięty.");
                    observedTags.Remove(TappedHashtag);
                }
                else
                {
                    await StatusBarManager.ShowText("Nie udało się usunąć tagu " + TappedHashtag + ".");
                }
            }
            else
            {
                var success = await App.ApiService.observeTag(TappedHashtag);
                if (success)
                {
                    await StatusBarManager.ShowText("Tag " + TappedHashtag + " został dodany.");
                    observedTags.Add(TappedHashtag);
                }
                else
                {
                    await StatusBarManager.ShowText("Nie udało się dodać tagu " + TappedHashtag + ".");
                }
            }
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
                await StatusBarManager.ShowText("Tag " + TappedHashtag + " został zablokowany.");
            }
            else
            {
                await StatusBarManager.ShowText("Nie udało się zablokować tagu " + TappedHashtag + ".");
            }
        }
        #endregion
    }
}
