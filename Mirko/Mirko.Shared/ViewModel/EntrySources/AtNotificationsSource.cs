using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Mirko.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WykopSDK.API.Models;

namespace Mirko.ViewModel
{
    public class AtNotificationsSource : IIncrementalSource<NotificationViewModel>
    {
        private uint pageIndex = 1;

        public void ClearCache()
        {

        }

        public async Task<IEnumerable<NotificationViewModel>> GetPagedItems(int pageSize, CancellationToken ct)
        {
            var VM = SimpleIoc.Default.GetInstance<NotificationsViewModel>();

            var supportedTypes = new NotificationType[]
            {
                NotificationType.Observe, NotificationType.Unobserve,
                NotificationType.CommentDirected, NotificationType.EntryDirected,
                NotificationType.System, NotificationType.Badge,
            };

            IEnumerable<Notification> notifications = null;
            IEnumerable<Notification> uniqueNotifications = null;

            notifications = await App.ApiService.GetNotifications(pageIndex++);

            if (notifications == null)
                return null;

            uniqueNotifications = notifications.Where(x => supportedTypes.Contains(x.Type));

            if (VM.AtNotifications.Count > 0)
            {
                var currentIDs = VM.AtNotifications.Select(x => x.Data.ID);
                uniqueNotifications = uniqueNotifications.Where(x => !currentIDs.Contains(x.ID));
            }

            if (uniqueNotifications.Count() == 0 && VM.AtNotifications.Count == 0)
                DispatcherHelper.CheckBeginInvokeOnUI(() => VM.AtNotifications.HasNoItems = true);

            return uniqueNotifications.Select(x => new NotificationViewModel(x));
        }
    }
}

