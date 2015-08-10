using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using WykopSDK.API.Models;
using WykopSDK.Parsers;

namespace Mirko.ViewModel
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
            var author = d.AuthorName;

            if (Data.Comments != null)
            {
                Comments = new ObservableCollectionEx<CommentViewModel>();
                foreach (var com in Data.Comments)
                    Comments.Add(new CommentViewModel(com) { RootEntryAuthor = author });

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
                    RefreshCommand.Execute(null);
            }
            else
            {
                Messenger.Default.Send<EntryViewModel>(this, "Entry UserControl");
            }
            SimpleIoc.Default.GetInstance<NavigationService>().NavigateTo("EntryPage");
        }

        private RelayCommand _favouriteCommand = null;
        [JsonIgnore]
        public RelayCommand FavouriteCommand
        {
            get { return _favouriteCommand ?? (_favouriteCommand = new RelayCommand(ExecuteFavouriteCommand)); }
        }

        private async void ExecuteFavouriteCommand()
        {
            var reply = await App.ApiService.AddToFavourites(Data.ID);
            Data.Favourite = reply.user_favorite;
        }

        private RelayCommand _shareCommand = null;
        [JsonIgnore]
        public RelayCommand ShareCommand
        {
            get { return _shareCommand ?? (_shareCommand = new RelayCommand(ExecuteShareCommand)); }
        }

        private void ExecuteShareCommand()
        {
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager,
                DataRequestedEventArgs>(ShareLinkHandler);

            DataTransferManager.ShowShareUI();
        }

        private void ShareLinkHandler(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var url = "http://wykop.pl/wpis/" + Data.ID;
            var body = HtmlToText.Convert(Data.Text.Replace("\n", " "));
            var splittedBody = new List<string>(body.Split(' '));
            const int wordsToGet = 4;

            var title = String.Join(" ", splittedBody.GetRange(0, (splittedBody.Count >= wordsToGet) ? wordsToGet : splittedBody.Count));
            if (splittedBody.Count > wordsToGet)
                title += "...";

            DataRequest request = args.Request;
            request.Data.Properties.Title = title;
            request.Data.SetWebLink(new Uri(url));
        }
    }
}
