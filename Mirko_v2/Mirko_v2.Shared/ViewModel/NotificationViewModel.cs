using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;
using WykopAPI.Models;

namespace Mirko_v2.ViewModel
{
    public class NotificationViewModel : ViewModelBase
    {
        public Notification Data { get; set; }

        public NotificationViewModel(Notification n)
        {
            Data = n;
            n = null;
        }

        private RelayCommand _markAsReadCommand = null;
        public RelayCommand MarkAsReadCommand
        {
            get { return _markAsReadCommand ?? (_markAsReadCommand = new RelayCommand(ExecuteMarkAsReadCommand)); }
        }

        private async void ExecuteMarkAsReadCommand()
        {
            await App.ApiService.markAsReadNotification(Data.ID);
        }
    }
}
