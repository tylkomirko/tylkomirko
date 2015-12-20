using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Mirko.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WykopSDK.API.Models;
using WykopSDK.Parsers;
using WykopSDK.Utils;

namespace Mirko.ViewModel
{
    public class ConversationViewModel : NewEntryBaseViewModel
    {
        public Conversation Data { get; set; }
        private ObservableCollectionEx<PMViewModel> _messages = null;
        public ObservableCollectionEx<PMViewModel> Messages
        {
            get { return _messages ?? (_messages = new ObservableCollectionEx<PMViewModel>()); }
        }

        private string _tappedHashtag = null;
        [JsonIgnore]
        public string TappedHashtag
        {
            get { return _tappedHashtag; }
            set { Set(() => TappedHashtag, ref _tappedHashtag, value); }
        }

        private bool _isOnline = false;
        public bool IsOnline
        {
            get { return _isOnline; }
            set { Set(() => IsOnline, ref _isOnline, value); }
        }

        public ConversationViewModel()
        {
        }

        public ConversationViewModel(Conversation d)
        {
            Data = d;
            if (Data.Messages != null)
            {
                foreach (var pm in Data.Messages)
                    this.Messages.Add(new PMViewModel(pm));

                ProcessMessages();
                Data.Messages = null;
            }

            d = null;
        }

        private RelayCommand<string> _goToProfilePage = null;
        [JsonIgnore]
        public RelayCommand<string> GoToProfilePage
        {
            get { return _goToProfilePage ?? (_goToProfilePage = new RelayCommand<string>(ExecuteGoToProfilePage)); }
        }

        private void ExecuteGoToProfilePage(string username)
        {
            if (string.IsNullOrEmpty(username)) return;

            var profilesVM = SimpleIoc.Default.GetInstance<ProfilesViewModel>();
            profilesVM.GoToProfile.Execute(username);
        }

        public override async void ExecuteSendMessageCommand()
        {
            await StatusBarManager.ShowTextAndProgressAsync("Wysyłam wiadomość...");

            if (string.IsNullOrEmpty(NewEntry.Text))
                NewEntry.Text = " \n ";

            bool success = true;
            try
            {
                if (NewEntry.Files != null)
                {
                    foreach (var file in NewEntry.Files)
                    {
                        using (var fileStream = await file.OpenStreamForReadAsync())
                            success = await App.ApiService.SendPM(NewEntry, Data.AuthorName, fileStream, file.Name);
                    }
                }
                else
                {
                    success = await App.ApiService.SendPM(NewEntry, Data.AuthorName);
                }
            }
            catch(Exception e)
            {
                App.TelemetryClient.TrackException(e);
                StatusBarManager.HideProgress();
                return;
            }

            NewEntry.RemoveAttachment();
            if (success)
            {
                App.TelemetryClient.TrackEvent("New PM");

                await StatusBarManager.ShowTextAsync("Wiadomość została wysłana.");
                await ExecuteUpdateMessagesCommand();
                Messenger.Default.Send(new NotificationMessage("PM-Success"));
            }
            else
            {
                Messenger.Default.Send(new NotificationMessage("PM-Fail"));
                await StatusBarManager.ShowTextAsync("Wiadomość nie została wysłana.");
            }
        }

        private RelayCommand _updateMessagesCommand = null;
        [JsonIgnore]
        public RelayCommand UpdateMessagesCommand
        {
            get { return _updateMessagesCommand ?? (_updateMessagesCommand = new RelayCommand(async () => await ExecuteUpdateMessagesCommand())); }
        }

        private async Task ExecuteUpdateMessagesCommand()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => Data.LastMessage = "Pobieram...");

            await StatusBarManager.ShowProgressAsync();
            var pms = await App.ApiService.GetPMs(Data.AuthorName);
            if (pms == null || pms.Count == 0)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() => Data.LastMessage = "");
                await StatusBarManager.HideProgressAsync();
                return;
            }

            IEnumerable<PM> newMessages = null;
            if (Messages.Count > 0)
            {
                var lastMsgTime = Messages.Last().Data.Date;
                newMessages = pms.Where(x => x.Date > lastMsgTime);
            }
            else
            {                
                newMessages = pms;
            }

            await DispatcherHelper.RunAsync(() =>
            {
                var vms = new List<PMViewModel>(newMessages.Count());
                foreach (var pm in newMessages)
                    vms.Add(new PMViewModel(pm));

                this.Messages.AddRange(vms);
                vms.Clear();
                vms = null;

                Data.LastUpdate = this.Messages.Last().Data.Date;
                Data.LastMessage = HtmlToText.Convert(this.Messages.Last().Data.Text);
                ProcessMessages();
            });

            Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(Data.AuthorName, "Clear PM")); // clear PM notification

            await StatusBarManager.HideProgressAsync();

            Messenger.Default.Send<NotificationMessage>(new NotificationMessage("Sort-Save"));
        }

        private RelayCommand _loadLastMessageCommand = null;
        [JsonIgnore]
        public RelayCommand LoadLastMessageCommand
        {
            get { return _loadLastMessageCommand ?? (_loadLastMessageCommand = new RelayCommand(ExecuteLoadLastMessageCommand)); }
        }

        private async void ExecuteLoadLastMessageCommand()
        {
            if (!string.IsNullOrEmpty(Data.LastMessage)) return;

            await ExecuteUpdateMessagesCommand();

            if (Messages.Count > 0)
            {
                var lastMsg = Messages.Last();
                var text = HtmlToText.Convert(lastMsg.Data.Text);
                var selected = text.FirstWords(20);
                if (text.Length > selected.Length)
                    text = selected + "...";

                if (string.IsNullOrWhiteSpace(text) && lastMsg.EmbedVM != null)
                    text = "(obraz)";

                Data.LastMessage = text;
            }
            else
            {
                Data.LastMessage = "";
            }
        }

        private RelayCommand _deleteConversation = null;
        [JsonIgnore]
        public RelayCommand DeleteConversation
        {
            get { return _deleteConversation ?? (_deleteConversation = new RelayCommand(ExecuteDeleteConversation)); }
        }

        private async void ExecuteDeleteConversation()
        {
            await StatusBarManager.ShowProgressAsync();
            bool success = await App.ApiService.DeleteConversation(Data.AuthorName);
            if(success)
            {
                Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>(Data.AuthorName, "Remove"));
                await StatusBarManager.ShowTextAsync("Rozmowa została usunięta.");
            }
            else
            {
                await StatusBarManager.ShowTextAsync("Nie udało się usunąć rozmowy.");
            }
        }

        private void ProcessMessages()
        {
            var maxIndex = this.Messages.Count - 1;
            var previousAuthor = "";
            var pms = this.Messages;

            for (int i = 0; i < this.Messages.Count; i++)
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

        /*
        private IEnumerable<PM> FilterMessages(List<PM> col)
        {
            var lastDate = this.Messages.Last().Data.Date;
            var newMsg = col.Where(x => x.Date >= lastDate).ToList();
            var ret = this.Messages.Select(x => x.Data).ToList();
            ret.AddRange(newMsg);

            // first find duplicates
            var duplicates = ret
                .GroupBy(i => i.Text)
                .Where(g => g.Count() > 1)
                .Select(g => g);

            // now group them
            foreach(var group in duplicates)
            {
                var key = group.Key;

                var firstMessage = group.First();
                var direction = firstMessage.Direction;
                var date = firstMessage.Date;

                var messagesToRemove = group.
                    Where(x => x.Direction == direction).
                    Where(x => Math.Abs(x.Date.Subtract(date).TotalSeconds) <= 120.0);

                foreach (var msg in messagesToRemove)
                    newMsg.Remove(msg);
            }

            return newMsg;
        }*/

        private RelayCommand _checkIfOnline = null;
        [JsonIgnore]
        public RelayCommand CheckIfOnline
        {
            get { return _checkIfOnline ?? (_checkIfOnline = new RelayCommand(ExecuteCheckIfOnline)); }
        }

        private void ExecuteCheckIfOnline()
        {
            IsOnline = false;
        }
    }
}
