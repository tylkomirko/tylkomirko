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
    public class EntryViewModel : EntryBaseViewModel
    {
        public Entry Data { get; set; }
        public ObservableCollectionEx<CommentViewModel> Comments { get; set; }
        
        public EntryViewModel()
        {

        }

        public EntryViewModel(Entry d) : base(d)
        {
            Data = d;

            if (Data.Comments != null)
            {
                Comments = new ObservableCollectionEx<CommentViewModel>();
                foreach (var com in Data.Comments)
                    Comments.Add(new CommentViewModel(com));

                Data.Comments = null;
            }

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
    }
}
