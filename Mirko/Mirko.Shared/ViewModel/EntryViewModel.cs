using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using WykopSDK.API.Models;

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
            }
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
    }
}
