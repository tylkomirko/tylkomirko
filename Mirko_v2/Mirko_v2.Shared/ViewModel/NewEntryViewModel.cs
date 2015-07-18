using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using Windows.Storage;
using Windows.Storage.Pickers;
using WykopAPI.Models;
using Mirko_v2.Utils;
using GalaSoft.MvvmLight.Messaging;

namespace Mirko_v2.ViewModel
{
    public class NewEntryViewModel : ViewModelBase, IFileOpenPickerContinuable
    {
        private NavigationService NavService = null;

        private uint _rootEntryID = 0;
        public uint RootEntryID
        {
            get { return _rootEntryID; }
            set { Set(() => RootEntryID, ref _rootEntryID, value); }
        }
        public EntryBaseViewModel Entry { get; set; }

        private NewEntry _data = null;
        public NewEntry Data
        {
            get { return _data ?? (_data = new NewEntry()); }
            set { Set(() => Data, ref _data, value); }
        }

        public NewEntryViewModel(NavigationService nav)
        {
            NavService = nav;
        }


        public void GoToNewEntryPage()
        {
            NavService.NavigateTo("NewEntryPage");
        }

        private RelayCommand _addAttachment = null;
        public RelayCommand AddAttachment
        {
            get { return _addAttachment ?? (_addAttachment = new RelayCommand(() => NavService.NavigateTo("AttachmentPage"))); }
        }

        private RelayCommand _openPicker = null;
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
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            openPicker.PickSingleFileAndContinue();
        }

        public async void ContinueFileOpenPicker(Windows.ApplicationModel.Activation.FileOpenPickerContinuationEventArgs args)
        {
            if (args.Files.Count() > 0)
            {
                StorageFile file = args.Files[0];
                var s = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                Data.FileStream = s.AsStreamForRead();
                Data.FileName = file.Name;
                Data.AttachmentName = file.DisplayName;

                NavService.GoBack();
            }
        }

        private RelayCommand _acceptAttachments = null;
        public RelayCommand AcceptAttachments
        {
            get { return _acceptAttachments ?? (_acceptAttachments = new RelayCommand(() => NavService.GoBack())); }
        }

        public void RemoveAttachment()
        {
            Data.RemoveAttachment();
        }

        private RelayCommand _sendMessageCommand = null;
        public RelayCommand SendMessageCommand
        {
            get { return _sendMessageCommand ?? (_sendMessageCommand = new RelayCommand(ExecuteSendMessageCommand)); }
        }

        private async void ExecuteSendMessageCommand()
        {
            await StatusBarManager.ShowTextAndProgressAsync("Wysyłam wiadomość...");

            Data.ID = RootEntryID;
            uint entryID = await App.ApiService.editEntry(Data);

            if (entryID != 0)
            {
                await StatusBarManager.ShowTextAsync("Dodano wpis.");
                Data.RemoveAttachment();
                Data.Text = null;

                NavService.GoBack();

                var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();
                if(RootEntryID == 0)
                {
                    var entry = await App.ApiService.getEntry(entryID);
                    if (entry != null)
                        mainVM.MirkoEntries.Insert(0, new EntryViewModel(entry));
                }
                else
                {
                    var entry = await App.ApiService.getEntry(RootEntryID);
                    if(entry != null)
                        Messenger.Default.Send<EntryViewModel>(new EntryViewModel(entry), "Update");
                }
                
            }
            else
            {
                await StatusBarManager.ShowTextAsync("Nie udało się dodać wpisu.");
            }
        }
    }
}
