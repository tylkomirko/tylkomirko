using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Mirko_v2.Utils;
using Newtonsoft.Json;
using WykopAPI.Models;

namespace Mirko_v2.ViewModel
{
    public class CommentViewModel : ViewModelBase
    {
        public EntryComment Data { get; set; }
        public EmbedViewModel EmbedVM { get; set; }

        public CommentViewModel()
        {

        }

        public CommentViewModel(EntryComment c)
        {
            Data = c;
            EmbedVM = new EmbedViewModel(Data.Embed);

            Data.Embed = null;
            c = null;
        }

        private RelayCommand _voteCommand = null;
        [JsonIgnore]
        public RelayCommand VoteCommand
        {
            get { return _voteCommand ?? (_voteCommand = new RelayCommand(ExecuteVoteCommand)); }
        }

        private async void ExecuteVoteCommand()
        {
            await StatusBarManager.ShowProgress();
            var reply = await App.ApiService.voteEntry(id: Data.EntryID, commentID: Data.ID, upVote: !Data.Voted, isItEntry: false);
            if (reply != null)
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
    }
}
