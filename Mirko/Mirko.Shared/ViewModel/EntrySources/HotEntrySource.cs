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
    public class HotEntrySource : IIncrementalSource<EntryViewModel>
    {
        private List<Entry> cache = new List<Entry>(50);
        private int pageIndex = 0;

        public void ClearCache()
        {
            cache.Clear();
            pageIndex = 0;
        }

        public async Task<IEnumerable<EntryViewModel>> GetPagedItems(int pageSize, CancellationToken ct)
        {
            var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();
            var entriesToReturn = new List<Entry>(pageSize);
            int entriesInCache = cache.Count();
            int missingEntries = pageSize - entriesInCache;
            int downloadedEntriesCount = 0;
            missingEntries = Math.Max(0, missingEntries);

            if (ct.IsCancellationRequested)
                return null;

            if (entriesInCache > 0)
            {
                int itemsToMove;
                if (entriesInCache >= pageSize)
                    itemsToMove = pageSize;
                else
                    itemsToMove = entriesInCache;

                entriesToReturn.AddRange(cache.GetRange(0, itemsToMove));

                this.cache.RemoveRange(0, itemsToMove);
            }

            ct.ThrowIfCancellationRequested();

            if (missingEntries > 0)
            {
                var timeSpan = mainVM.HotTimeSpan;
                var entries = new List<Entry>(50);

                IEnumerable<Entry> newEntries = null;
                if (App.ApiService.IsNetworkAvailable)
                {
                    await StatusBarManager.ShowTextAndProgressAsync("Pobieram wpisy...");

                    do
                    {
                        if (timeSpan >= 6)
                        {
                            newEntries = await App.ApiService.getHotEntries(timeSpan, pageIndex++, ct);
                        }
                        else
                        {
                            var newEntries_temp = await App.ApiService.getHotEntries(6, pageIndex++, ct);
                            var limitingTime = DateTime.UtcNow.AddHours(-timeSpan);
                            newEntries = newEntries_temp.Where(x => x.Date.Subtract(App.OffsetUTCInPoland) > limitingTime);
                        }

                        if (newEntries != null)
                        {
                            if (mainVM.HotEntries.Count > 0 || entries.Count > 0 || entriesToReturn.Count > 0)
                            {
                                // this is to avoid duplicates.
                                var currentIDs = mainVM.HotEntries.Select(x => x.Data.ID).ToList();
                                currentIDs.AddRange(entries.Select(x => x.ID));
                                currentIDs.AddRange(entriesToReturn.Select(x => x.ID));

                                newEntries = newEntries.Where(x => !currentIDs.Contains(x.ID));
                            }

                            entries.AddRange(newEntries);
                        }

                        if (pageIndex >= 12 || (newEntries != null && newEntries.Count() == 0))
                        {
                            DispatcherHelper.CheckBeginInvokeOnUI(() => mainVM.HotEntries.HasMoreItems = false);
                            if (mainVM.HotEntries.Count == 0 && entries.Count == 0)
                                DispatcherHelper.CheckBeginInvokeOnUI(() => mainVM.HotEntries.HasNoItems = true);
                            break;
                        }

                    } while (entries.Count <= missingEntries && newEntries != null);

                    await StatusBarManager.HideProgressAsync();
                }
                else
                {
                    // offline mode
                    if (mainVM.HotEntries.Count == 0)
                    {
                        await StatusBarManager.ShowTextAndProgressAsync("Wczytuje wpisy...");
                        var savedEntries = await mainVM.ReadCollection("HotEntries");
                        await StatusBarManager.HideProgressAsync();

                        return savedEntries;
                    }
                    else
                    {
                        return null;
                    }
                }

                var tmp = new List<Entry>(missingEntries);
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

                entriesToReturn.AddRange(tmp);
                cache.AddRange(entries);
                entries.Clear();
            }

            var VMs = new List<EntryViewModel>(entriesToReturn.Count);
            foreach (var entry in entriesToReturn)
                VMs.Add(new EntryViewModel(entry));

            return VMs;
        }
    }
}
