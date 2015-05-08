using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using WykopAPI.Models;
using Mirko_v2.Utils;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using System.Collections.ObjectModel;
using Windows.Storage;
using System.IO;
using Windows.Storage.Pickers;

namespace Mirko_v2.ViewModel
{
    public class ConversationViewModel : ViewModelBase, IFileOpenPickerContinuable
    {
        public Conversation Data { get; set; }
        public ObservableCollectionEx<PMViewModel> Messages { get; set; }

        private NewEntry _newEntry = null;
        public NewEntry NewEntry
        {
            get { return _newEntry ?? (_newEntry = new NewEntry()); }
            set { Set(() => NewEntry, ref _newEntry, value); }
        }

        public ConversationViewModel(Conversation d)
        {
            Data = d;
            if (Data.Messages != null)
            {
                Messages = new ObservableCollectionEx<PMViewModel>();
                foreach (var pm in Data.Messages)
                    Messages.Add(new PMViewModel(pm));

                ProcessMessages(Messages);
                Data.Messages = null;
            }

            d = null;
        }

        private RelayCommand _sendMessageCommand = null;
        public RelayCommand SendMessageCommand
        {
            get { return _sendMessageCommand ?? (_sendMessageCommand = new RelayCommand(ExecuteSendMessageCommand)); }
        }

        private async void ExecuteSendMessageCommand()
        {
            await StatusBarManager.ShowTextAndProgress("Wysyłam wiadomość...");
            bool success = await App.ApiService.sendPM(NewEntry, Data.AuthorName);
            if (success)
            {
                await StatusBarManager.ShowText("Wiadomość została wysłana.");
                Data.LastMessage = NewEntry.Text;
                Data.LastUpdate = DateTime.Now;
            }
            else
            {
                await StatusBarManager.ShowText("Wiadomość nie została wysłana.");
            }
        }

        private RelayCommand _loadLastMessageCommand = null;
        public RelayCommand LoadLastMessageCommand
        {
            get { return _loadLastMessageCommand ?? (_loadLastMessageCommand = new RelayCommand(ExecuteLoadLastMessageCommand)); }
        }

        private async void ExecuteLoadLastMessageCommand()
        {
            if (!string.IsNullOrEmpty(Data.LastMessage)) return;

            var userName = Data.AuthorName;
            Data.LastMessage = "Pobieram...";
            var pms = await App.ApiService.getPMs(userName);
            if (pms == null) return;

            Messages = new ObservableCollectionEx<PMViewModel>();
            foreach (var pm in pms)
                Messages.Add(new PMViewModel(pm));

            var lastPMText = pms.Last().Text;

            Data.LastMessage = HTMLUtils.HTMLtoTEXT(lastPMText);
        }

        private RelayCommand _addAttachment = null;
        public RelayCommand AddAttachment
        {
            get { return _addAttachment ?? (_addAttachment = new RelayCommand(ExecuteAddAttachment)); }
        }

        private void ExecuteAddAttachment()
        {
            var navService = SimpleIoc.Default.GetInstance<INavigationService>();
            navService.NavigateTo("AddAttachmentPage", "PM");
        }

        private void ProcessMessages(ObservableCollectionEx<PMViewModel> pms)
        {
            var maxIndex = pms.Count - 1;
            var previousAuthor = "";

            for (int i = 0; i < pms.Count; i++)
            {
                var pm = pms[i];
                var pmData = pm.Data;

                if (pmData.AuthorName == previousAuthor)
                {
                    if (pmData.Direction == MessageDirection.Received)
                    {
                        while (i < maxIndex && pms[i + 1].Data.AuthorName == pmData.AuthorName)
                            i++;
                    }
                    else
                    {
                        pms[i - 1].ShowArrow = false;
                        while (i < maxIndex && pms[i + 1].Data.AuthorName == pmData.AuthorName)
                            i++;

                        pms[i].ShowArrow = true;
                    }
                }
                else
                {
                    pms[i].ShowArrow = true;
                    previousAuthor = pmData.AuthorName;
                }

                if (pms[maxIndex].Data.Direction == MessageDirection.Sent)
                    pms[maxIndex].ShowArrow = false;
            }
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
                NewEntry.FileStream = s.AsStreamForRead();
                NewEntry.FileName = file.Name;
                NewEntry.AttachmentName = file.DisplayName;

                SimpleIoc.Default.GetInstance<INavigationService>().GoBack();
            }
        }

        private RelayCommand _removeAttachment = null;
        public RelayCommand RemoveAttachment
        {
            get { return _removeAttachment ?? (_removeAttachment = new RelayCommand(() => NewEntry.RemoveAttachment())); }
        }

        private RelayCommand _acceptPressed = null;
        public RelayCommand AcceptPressed
        {
            get { return _acceptPressed ?? (_acceptPressed = new RelayCommand(ExecuteAcceptPressed)); }
        }

        private void ExecuteAcceptPressed()
        {
            SimpleIoc.Default.GetInstance<INavigationService>().GoBack();
        }
    }
}
