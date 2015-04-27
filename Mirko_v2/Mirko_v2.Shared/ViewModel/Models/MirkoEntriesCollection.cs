using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using WykopAPI.Models;
using Mirko_v2.Utils;

namespace Mirko_v2.ViewModel
{
    public class MirkoEntrySource : IIncrementalSource<Entry>
    {
        private List<Entry> cache = new List<Entry>(50);

        public async Task<IEnumerable<Entry>> GetPagedItems(int pageIndex, int pageSize)
        {
            //StatusBarManager.ShowTextAndProgress("Pobieram wpisy...");
            // FIXME

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
                var entries = new List<Entry>(50);

                IEnumerable<Entry> newEntries = null;
                //if (App.IsNetworkAvailable)
                if(true)
                {
                    do
                    {
                        newEntries = await App.ApiService.getEntries(pageIndex++);

                        if (newEntries != null)
                        {
                            entries.AddRange(newEntries);

                            //await App.SQLite.InsertEntries(newEntries);
                        }

                    } while (entries.Count <= missingEntries && newEntries != null);
                }
                else
                {
                    // network not avaible. use sqlite.
                    /*
                    do
                    {
                        List<SQLiteStorage.Tables.Entry> dbTemp = null;
                        if (FirstID == null)
                            dbTemp = await App.SQLite.SelectMultiple("MainEntries", int.MaxValue);
                        else
                            dbTemp = await App.SQLite.SelectMultiple("MainEntries", FirstID.Value);

                        newEntries = SQLiteStorage.Converters.DBToJson(dbTemp);

                        if (newEntries != null)
                        {
                            entries.AddRange(newEntries);
                        }
                        else
                        {
                            _hasMoreItems = false;
                            StatusBarManager.HideProgress();
                            return new LoadMoreItemsResult() { Count = 0 };
                        }

                    } while (entries.Count <= missingEntries && newEntries != null);
                     * */
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

            // FIXME
            //StatusBarManager.HideProgress();

            return entriesToReturn;
        }
    }
}
