using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WykopAPI.Models;

namespace Mirko_v2.ViewModel
{
    public class FavEntrySource : IIncrementalSource<EntryViewModel>
    {
        private List<Entry> cache = new List<Entry>(50);
        private bool entriesDownloaded = false;

        public void ClearCache()
        {
            cache.Clear();
        }

        public async Task<IEnumerable<EntryViewModel>> GetPagedItems(int pageSize, CancellationToken ct)
        {
            var entriesToReturn = new List<Entry>(pageSize);
            int entriesInCache = cache.Count();
            int missingEntries = pageSize - entriesInCache;
            int downloadedEntriesCount = 0;
            missingEntries = Math.Max(0, missingEntries);

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

            if (missingEntries > 0)
            {
                var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();
                var entries = new List<Entry>(50);

                IEnumerable<Entry> newEntries = null;
                if (App.ApiService.IsNetworkAvailable && !entriesDownloaded)
                {
                    await StatusBarManager.ShowTextAndProgress("Pobieram wpisy...");

                    newEntries = await App.ApiService.getFavourites();
                    if (newEntries != null)
                    {
                        entriesDownloaded = true;
                        if (newEntries.Count() == 0)
                        {
                            mainVM.FavEntries.HasNoItems = true;
                            mainVM.FavEntries.HasMoreItems = false;
                        }
                        else
                        {
                            entries.AddRange(newEntries);
                        }
                    }

                    await StatusBarManager.HideProgress();
                }
                else if(!App.ApiService.IsNetworkAvailable)
                {
                    // offline mode
                    if(mainVM.FavEntries.Count == 0)
                    {
                        await StatusBarManager.ShowTextAndProgress("Wczytuje wpisy...");
                        var savedEntries = await mainVM.ReadCollection("FavEntries");
                        await StatusBarManager.HideProgress();

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
            foreach(var entry in entriesToReturn)
                VMs.Add(new EntryViewModel(entry));

            return VMs;
        }
    }
}
