using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;
using WykopSDK.API.Models;

namespace Mirko.ViewModel
{
    public abstract class NewEntryBaseViewModel : ViewModelBase
#if WINDOWS_PHONE_APP
        , IFileOpenPickerContinuable
#endif

    {
        protected bool _busy = false;
        protected bool Busy
        {
            get { return _busy; }
            set { _busy = value; SendMessageCommand.RaiseCanExecuteChanged(); }
        }

        private NewEntry _newEntry = null;
        public NewEntry NewEntry
        {
            get { return _newEntry ?? (_newEntry = new NewEntry()); }
            set { Set(() => NewEntry, ref _newEntry, value); }
        }

        public NewEntryBaseViewModel()
        {
            NewEntry.PropertyChanged += NewEntry_PropertyChanged;
        }

        private void NewEntry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NewEntry.Text))
                SendMessageCommand.RaiseCanExecuteChanged();
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

#if WINDOWS_PHONE_APP
        private void ExecuteOpenPicker()
#else
        private async void ExecuteOpenPicker()
#endif
        {
            var openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

#if WINDOWS_PHONE_APP
            if(NewEntry.EntryID == 0)
                openPicker.PickMultipleFilesAndContinue();
            else
                openPicker.PickSingleFileAndContinue();
#else
            var files = new List<StorageFile>();

            if (NewEntry.EntryID == 0)
                files.AddRange(await openPicker.PickMultipleFilesAsync());
            else
                files.Add(await openPicker.PickSingleFileAsync());

            ProcessFiles(files);
#endif
        }

        private void ProcessFiles(IList<StorageFile> files)
        {
            if (files.Count == 0)
                return;

            NewEntry.Files = files.ToArray();

            if (files.Count() == 1)
                NewEntry.AttachmentName = files[0].DisplayName;
            else
                NewEntry.SetAttachmentName(files.Count);

            SimpleIoc.Default.GetInstance<NavigationService>().GoBack();
        }

#if WINDOWS_PHONE_APP
        public void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            if(args.Files != null)
                ProcessFiles(args.Files.ToList());
        }
#endif

        private RelayCommand _sendMessageCommand = null;
        [JsonIgnore]
        public RelayCommand SendMessageCommand
        {
            get { return _sendMessageCommand ?? (_sendMessageCommand = new RelayCommand(ExecuteSendMessageCommand, SendMessageCanExecute)); }
        }

        private bool SendMessageCanExecute()
        {
            if (Busy)
                return false;
            else if (!string.IsNullOrEmpty(NewEntry.Text) || !string.IsNullOrEmpty(NewEntry.AttachmentName))
                return true;
            else
                return false;
        }

        public abstract void ExecuteSendMessageCommand();
    }
}
