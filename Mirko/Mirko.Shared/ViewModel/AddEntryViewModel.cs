using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WykopSDK.API.Models;

namespace Mirko.ViewModel
{
    public class AddEntryViewModel : ViewModelBase
    {
        private NewEntry _newEntry = null;
        public NewEntry NewEntry
        {
            get { return _newEntry ?? (_newEntry = new NewEntry()); }
            set { Set(() => NewEntry, ref _newEntry, value); }
        }

        private RelayCommand _removeAttachment = null;
        public RelayCommand RemoveAttachment
        {
            get { return _removeAttachment ?? (_removeAttachment = new RelayCommand(() => NewEntry.RemoveAttachment())); }
        }
    }
}
