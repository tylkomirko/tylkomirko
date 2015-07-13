using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Mirko_v2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WykopAPI.Models;

namespace Mirko_v2.ViewModel
{
    public class MyEntrySource : IIncrementalSource<EntryViewModel>
    {
        private List<Entry> cache = new List<Entry>(50);
        private int pageIndex = 1;

        public void ClearCache()
        {
            cache.Clear();
            pageIndex = 1;
        }

        public async Task<IEnumerable<EntryViewModel>> GetPagedItems(int pageSize, CancellationToken ct)
        {
            var entriesToReturn = new List<Entry>(pageSize);
            int entriesInCache = cache.Count();
            int missingEntries = pageSize - entriesInCache;
            int downloadedEntriesCount = 0;
            missingEntries = Math.Max(0, missingEntries);
            var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();

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
                var entries = new List<Entry>(10);

                IEnumerable<Entry> newEntries = null;
                if (App.ApiService.IsNetworkAvailable)
                {
                    await StatusBarManager.ShowTextAndProgressAsync("Pobieram wpisy...");

                    do
                    {                       
                        newEntries = await App.ApiService.getMyEntries(pageIndex++, ct);
                        if (newEntries != null)
                            entries.AddRange(newEntries);

                        if (newEntries == null || newEntries.Count() == 0)
                        {
                            await DispatcherHelper.RunAsync(() => mainVM.MyEntries.HasMoreItems = false);
                            if(mainVM.MyEntries.Count == 0 && pageIndex == 2)
                                await DispatcherHelper.RunAsync(() => mainVM.MyEntries.HasNoItems = true);

                            break;
                        }

                    } while (entries.Count <= missingEntries);

                    await StatusBarManager.HideProgressAsync();
                }
                else
                {
                    // offline mode
                    if (mainVM.MyEntries.Count == 0)
                    {
                        await StatusBarManager.ShowTextAndProgressAsync("Wczytuje wpisy...");
                        var savedEntries = await mainVM.ReadCollection("MyEntries");
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
