using Mirko.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using WykopSDK.API.Models;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using System.Threading;

namespace Mirko.ViewModel
{
    public class TaggedEntrySource : IIncrementalSource<EntryViewModel>
    {
        private List<Entry> cache = new List<Entry>(25);
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

            if (missingEntries > 0)
            {                
                var tag = mainVM.SelectedHashtag.Hashtag;
                var entries = new List<Entry>(25);

                if (App.ApiService.IsNetworkAvailable)
                {
                    await StatusBarManager.ShowTextAndProgressAsync("Pobieram wpisy...");

                    do
                    {
                        var newEntriesTemp = await App.ApiService.getTaggedEntries(tag, pageIndex++);
                        if (newEntriesTemp != null)
                        {
                            if (newEntriesTemp.Entries.Count > 0)
                            {
                                entries.AddRange(newEntriesTemp.Entries);
                                await DispatcherHelper.RunAsync(() => mainVM.SelectedHashtag = newEntriesTemp.Meta);
                            }
                            else
                                break;
                        }
                        else
                        {
                            break;
                        }

                    } while (entries.Count <= missingEntries);

                    await StatusBarManager.HideProgressAsync();
                }
                else
                {
                    return null;
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

            if (entriesToReturn.Count == 0 && mainVM.TaggedEntries.Count == 0)
                await DispatcherHelper.RunAsync(() => mainVM.TaggedEntries.HasNoItems = true);

            var VMs = new List<EntryViewModel>(entriesToReturn.Count);
            foreach (var entry in entriesToReturn)
                VMs.Add(new EntryViewModel(entry));

            return VMs;
        }
    }
}
