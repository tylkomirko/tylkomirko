using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Mirko.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WykopSDK.API.Models;

namespace Mirko.ViewModel
{
    public class AtNotificationsSource : IIncrementalSource<NotificationViewModel>
    {
        private List<Notification> cache = new List<Notification>(50);
        private uint pageIndex = 1;
        private uint lastID = uint.MaxValue;

        public void ClearCache()
        {
            
        }

        public async Task<IEnumerable<NotificationViewModel>> GetPagedItems(int pageSize, CancellationToken ct)
        {
            var notificationsToReturn = new List<Notification>(pageSize);
            int entriesInCache = this.cache.Count();
            int missingEntries = pageSize - entriesInCache;
            int downloadedEntriesCount = 0;
            missingEntries = Math.Max(0, missingEntries);
            var VM = SimpleIoc.Default.GetInstance<NotificationsViewModel>();

            if (entriesInCache > 0)
            {
                int itemsToMove;
                if (entriesInCache >= pageSize)
                    itemsToMove = pageSize;
                else
                    itemsToMove = entriesInCache;

                var temp = new List<Notification>(itemsToMove);
                for (int i = 0; i < itemsToMove; i++)
                    temp.Add(this.cache[i]);

                notificationsToReturn.AddRange(cache.GetRange(0, itemsToMove));

                this.cache.RemoveRange(0, itemsToMove);
            }

            if (missingEntries > 0)
            {
                var entries = new List<Notification>(50);
                var supportedTypes = new NotificationType[] 
                { 
                    NotificationType.Observe, NotificationType.Unobserve, 
                    NotificationType.CommentDirected, NotificationType.EntryDirected,
                    NotificationType.System, NotificationType.Badge,
                };

                do
                {
                    var temp = await App.ApiService.getNotifications(pageIndex++);

                    if (temp == null || (temp != null && temp.Count == 0 && pageIndex == 2))
                        return null;

                    IEnumerable<Notification> unique = null;
                    if (VM.AtNotifications.Count > 0)
                    {
                        var currentIDs = VM.AtNotifications.Select(x => x.Data.ID);
                        unique = temp.Where(x => supportedTypes.Contains(x.Type)).Where(x => !currentIDs.Contains(x.ID));
                    }
                    else
                    {
                        unique = temp.Where(x => x.ID < lastID && supportedTypes.Contains(x.Type));
                    }

                    if (unique.Count() == 0)
                    {
                        if(entries.Count == 0 && VM.AtNotifications.Count == 0)
                            DispatcherHelper.CheckBeginInvokeOnUI(() => VM.AtNotifications.HasNoItems = true);
                        break;
                    }

                    entries.AddRange(unique);

                } while (entries.Count <= missingEntries);

                var tmp = new List<Notification>(missingEntries);

                if (entries.Count >= missingEntries)
                {
                    tmp.AddRange(entries.GetRange(0, missingEntries));

                    entries.RemoveRange(0, missingEntries);
                }
                else
                {
                    downloadedEntriesCount = entries.Count;

                    tmp.AddRange(entries);
                    entries.RemoveRange(0, downloadedEntriesCount);
                }

                notificationsToReturn.AddRange(tmp);
                this.cache.AddRange(entries);
                entries.Clear();
            }

            var VMs = new List<NotificationViewModel>(notificationsToReturn.Count);
            foreach (var n in notificationsToReturn)
                VMs.Add(new NotificationViewModel(n));

            return VMs;
        }
    }
}
