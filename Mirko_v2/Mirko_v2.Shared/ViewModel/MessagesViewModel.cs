﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Text;
using WykopAPI.Models;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight.Ioc;
using Windows.Storage;

namespace Mirko_v2.ViewModel
{
    public class MessagesViewModel : ViewModelBase
    {
        private ObservableCollectionEx<ConversationViewModel> _conversationsList = null;
        public ObservableCollectionEx<ConversationViewModel> ConversationsList
        {
            get { return _conversationsList ?? (_conversationsList = new ObservableCollectionEx<ConversationViewModel>()); }
        }

        private ConversationViewModel _currentConversation = null;
        public ConversationViewModel CurrentConversation
        {
            get { return _currentConversation; }
            set { Set(() => CurrentConversation, ref _currentConversation, value); }
        }

        private NewEntry _newMessage = null;
        public NewEntry NewMessage
        {
            get { return _newMessage ?? (_newMessage = new NewEntry()); }
            set { Set(() => NewMessage, ref _newMessage, value); }
        }

        private RelayCommand _goToConversationPageCommand = null;
        public RelayCommand GoToConversationPageCommand
        {
            get { return _goToConversationPageCommand ?? (_goToConversationPageCommand = new RelayCommand(ExecuteGoToConversationPageCommand)); }
        }

        private void ExecuteGoToConversationPageCommand()
        {
            var navService = SimpleIoc.Default.GetInstance<INavigationService>();
            navService.NavigateTo("ConversationPage");
        }

        private RelayCommand _saveCommand = null;
        public RelayCommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(ExecuteSaveCommand)); }
        }

        private async void ExecuteSaveCommand()
        {
            var list = new List<Conversation>(ConversationsList.Count);
            foreach(var conv in ConversationsList)
            {
                Conversation tmp = conv.Data;
                if(conv.Messages != null)
                    tmp.Messages = new List<PM>(conv.Messages.Select(x => x.Data));
                list.Add(tmp);
            }

            await App.ApiService.LocalStorage.SaveConversations(list);
        }

        private void SortConversationList()
        {
            var ordered = this.ConversationsList.OrderByDescending(x => x.Data.LastUpdate).ToList();

            for (int i = 0; i < ordered.Count; i++)
            {
                var item = ordered[i];
                var newIndex = i;
                var oldIndex = 0;

                foreach (var tmp in ConversationsList)
                {
                    if (tmp.Data.AuthorName == item.Data.AuthorName)
                        break;
                    else
                        oldIndex++;
                }

                if (oldIndex != newIndex)
                    this.ConversationsList.Move(oldIndex, newIndex);
            }
        }
    }
}