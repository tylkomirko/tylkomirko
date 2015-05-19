using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
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

        private string TappedHashtag = null;
        
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

        public void GoToEntryPage()
        {
            Messenger.Default.Send<EntryViewModel>(this, "Entry UserControl");
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
            var reply = await App.ApiService.voteEntry(id: Data.ID, upVote: !Data.Voted);

            Data.VoteCount = (uint)reply.vote;
            Data.Voted = !Data.Voted;
            Data.Voters = reply.Voters;
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

        private RelayCommand _goToHashtagPage = null;
        [JsonIgnore]
        public RelayCommand GoToHashtagPage
        {
            get { return _goToHashtagPage ?? (_goToHashtagPage = new RelayCommand(ExecuteGoToHashtagPage)); }
        }

        private void ExecuteGoToHashtagPage()
        {
            var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();

            mainVM.SelectedHashtag = new Meta() { Hashtag = TappedHashtag };
            mainVM.TaggedEntries.ClearAll();
            mainVM.TaggedNewEntries.Clear();

            SimpleIoc.Default.GetInstance<INavigationService>().NavigateTo("HashtagEntriesPage");
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
