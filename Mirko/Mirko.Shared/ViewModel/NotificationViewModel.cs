using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;
using WykopSDK.API.Models;

namespace Mirko.ViewModel
{
    public class NotificationViewModel : ViewModelBase, IComparable
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

        public int CompareTo(object obj)
        {
            var other = (NotificationViewModel)obj;

            return other.Data.ID.CompareTo(this.Data.ID);
        }
    }
}
