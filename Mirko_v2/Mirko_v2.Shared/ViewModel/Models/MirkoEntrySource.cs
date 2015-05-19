using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using WykopAPI.Models;
using Mirko_v2.Utils;
using GalaSoft.MvvmLight.Ioc;

namespace Mirko_v2.ViewModel
{
    public class MirkoEntrySource : IIncrementalSource<EntryViewModel>
    {
        private List<Entry> cache = new List<Entry>(50);
        private int pageIndex = 0;

        public void ClearCache()
        {
            cache.Clear();
            pageIndex = 0;
        }

        public async Task<IEnumerable<EntryViewModel>> GetPagedItems(int pageSize)
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
                if (App.ApiService.IsNetworkAvailable)
                {
                    await StatusBarManager.ShowTextAndProgress("Pobieram wpisy...");

                    do
                    {
                        newEntries = await App.ApiService.getEntries(pageIndex++);
                        if (newEntries != null)
                            entries.AddRange(newEntries);

                    } while (entries.Count <= missingEntries && newEntries != null);

                    await StatusBarManager.HideProgress();
                }
                else
                {
                    // offline mode
                    if(mainVM.MirkoEntries.Count == 0)
                    {
                        await StatusBarManager.ShowTextAndProgress("Wczytuje wpisy...");
                        var savedEntries = await mainVM.ReadCollection("MirkoEntries");
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
