using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Text;
using WykopAPI.Models;

namespace Mirko_v2.ViewModel
{
    public class EntryViewModel : ViewModelBase
    {
        public Entry Data { get; set; }
        public ObservableCollectionEx<CommentViewModel> Comments { get; set; }
        public EmbedViewModel EmbedVM { get; set; }

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
        public RelayCommand ReplyCommand
        {
            get { return _replyCommand ?? (_replyCommand = new RelayCommand(ExecuteReplyCommand)); }
        }

        private void ExecuteReplyCommand()
        {
            throw new System.NotImplementedException();
        }

        private RelayCommand _favouriteCommand = null;
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
        public RelayCommand EditCommand
        {
            get { return _editCommand ?? (_replyCommand = new RelayCommand(ExecuteEditCommand)); }
        }

        private void ExecuteEditCommand()
        {
            throw new System.NotImplementedException();
        }

        private RelayCommand _deleteCommand = null;
        public RelayCommand DeleteCommand
        {
            get { return _deleteCommand ?? (_replyCommand = new RelayCommand(ExecuteDeleteCommand)); }
        }

        private void ExecuteDeleteCommand()
        {
            throw new System.NotImplementedException();
        }
    }
}
