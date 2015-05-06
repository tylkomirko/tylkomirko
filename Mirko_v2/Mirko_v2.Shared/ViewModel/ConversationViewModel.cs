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

namespace Mirko_v2.ViewModel
{
    public class ConversationViewModel : ViewModelBase
    {
        public Conversation Data { get; set; }
        public ObservableCollectionEx<PMViewModel> Messages { get; set; }

        private string _newMessageText = null;
        public string NewMessageText
        {
            get { return _newMessageText ?? (_newMessageText = ""); }
            set { Set(() => NewMessageText, ref _newMessageText, value); }
        }

        private string _attachmentName = null;
        public string AttachmentName
        {
            get { return _attachmentName ?? (_attachmentName = ""); }
            set { Set(() => AttachmentName, ref _attachmentName, value); }
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
    }
}
