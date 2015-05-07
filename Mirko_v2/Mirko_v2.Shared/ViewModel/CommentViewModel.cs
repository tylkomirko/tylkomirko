using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;
using WykopAPI.Models;

namespace Mirko_v2.ViewModel
{
    public class CommentViewModel : ViewModelBase
    {
        public EntryComment Data { get; set; }
        public EmbedViewModel EmbedVM { get; set; }

        public CommentViewModel(EntryComment c)
        {
            Data = c;
            EmbedVM = new EmbedViewModel(Data.Embed);

            Data.Embed = null;
            c = null;
        }

        private RelayCommand _voteCommand = null;
        public RelayCommand VoteCommand
        {
            get { return _voteCommand ?? (_voteCommand = new RelayCommand(ExecuteVoteCommand)); }
        }

        private async void ExecuteVoteCommand()
        {
            var reply = await App.ApiService.voteEntry(id: Data.EntryID, commentID: Data.ID, upVote: !Data.Voted);

            Data.VoteCount = (uint)reply.vote;
            Data.Voted = !Data.Voted;
            Data.Voters = reply.Voters;
        }
    }
}
