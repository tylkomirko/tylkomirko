using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;
using WykopAPI.Models;

namespace Mirko_v2.ViewModel
{
    public class EntryViewModel : ViewModelBase
    {
        public Entry EntryData { get; set; }

        private RelayCommand<Entry> _replyCommand = null;
        public RelayCommand<Entry> ReplyCommand
        {
            get { return _replyCommand ?? (_replyCommand = new RelayCommand<Entry>(ExecuteReplyCommand)); }
        }

        private void ExecuteReplyCommand(Entry obj)
        {
            throw new System.NotImplementedException();
        }
    }
}
