using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;
using WykopAPI.Models;

namespace Mirko_v2.ViewModel
{
    public class NewEntryBaseViewModel : ViewModelBase, IFileOpenPickerContinuable
    {
        private NewEntry _newEntry = null;
        public NewEntry NewEntry
        {
            get { return _newEntry ?? (_newEntry = new NewEntry()); }
            set { Set(() => NewEntry, ref _newEntry, value); }
        }

        private RelayCommand _addAttachment = null;
        [JsonIgnore]
        public RelayCommand AddAttachment
        {
            get { return _addAttachment ?? (_addAttachment = new RelayCommand(ExecuteAddAttachment)); }
        }

        private void ExecuteAddAttachment()
        {
            var VM = SimpleIoc.Default.GetInstance<NewEntryViewModel>();
            if (this is ConversationViewModel)
                VM.AttachmentTarget = SimpleIoc.Default.GetInstance<MessagesViewModel>().CurrentConversation;
            else
                VM.AttachmentTarget = this;

            SimpleIoc.Default.GetInstance<NavigationService>().NavigateTo("AttachmentPage");
        }

        private RelayCommand _acceptAttachments = null;
        [JsonIgnore]
        public RelayCommand AcceptAttachments
        {
            get { return _acceptAttachments ?? (_acceptAttachments = new RelayCommand(() => SimpleIoc.Default.GetInstance<NavigationService>().GoBack())); }
        }

        private RelayCommand _openPicker = null;
        [JsonIgnore]
        public RelayCommand OpenPicker
        {
            get { return _openPicker ?? (_openPicker = new RelayCommand(ExecuteOpenPicker)); }
        }

        private void ExecuteOpenPicker()
        {
            var openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            openPicker.PickSingleFileAndContinue();
        }

        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            if (args.Files.Count() > 0)
            {
                StorageFile file = args.Files[0];
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                NewEntry.FileStream = stream.AsStreamForRead();
                NewEntry.FileName = file.Name;
                NewEntry.AttachmentName = file.DisplayName;

                SimpleIoc.Default.GetInstance<NavigationService>().GoBack();
            }
        }
    }
}
