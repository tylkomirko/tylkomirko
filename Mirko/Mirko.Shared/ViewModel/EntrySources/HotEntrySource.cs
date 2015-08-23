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
        // for some reason API starts counting pages from 1...
        private int pageIndex = 1;
        private const int maxPageIndex = 12;

        public void ClearCache()
        {
            pageIndex = 1;
        }

        public async Task<IEnumerable<EntryViewModel>> GetPagedItems(int pageSize, CancellationToken ct)
        {
            var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();

            if (ct.IsCancellationRequested)
                return null;

            var timeSpan = mainVM.HotTimeSpan;

            IEnumerable<Entry> newEntries = null;
            if (App.ApiService.IsNetworkAvailable)
            {
                await StatusBarManager.ShowTextAndProgressAsync("Pobieram wpisy...");

                if (timeSpan >= 6)
                {
                    newEntries = await App.ApiService.GetHotEntries(timeSpan, pageIndex++, ct);
                }
                else
                {
                    var limitingTime = DateTime.UtcNow.AddHours(-timeSpan);
                    newEntries = new List<Entry>();

                    while(newEntries.Count() < 10 && pageIndex <= maxPageIndex)
                    {
                        var newEntries_temp = await App.ApiService.GetHotEntries(6, pageIndex++, ct);
                        if (newEntries_temp != null)
                        {
                            var unique = newEntries_temp.Where(x => x.Date.Subtract(App.OffsetUTCInPoland) > limitingTime);
                            newEntries = newEntries.Concat(unique);
                        }
                    }
                }

                if (newEntries == null)
                    return null;

                if (pageIndex >= maxPageIndex || newEntries.Count() == 0)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() => mainVM.HotEntries.HasMoreItems = false);
                    if (mainVM.HotEntries.Count == 0 && newEntries.Count() == 0)
                        DispatcherHelper.CheckBeginInvokeOnUI(() => mainVM.HotEntries.HasNoItems = true);
                }

                await StatusBarManager.HideProgressAsync();

                ct.ThrowIfCancellationRequested();

                return newEntries.Select(x => new EntryViewModel(x));
            }
            else if (mainVM.HotEntries.Count == 0)
            {
                // offline mode
                await StatusBarManager.ShowTextAndProgressAsync("Wczytuje wpisy...");
                var savedEntries = await mainVM.ReadCollection("HotEntries");
                await StatusBarManager.HideProgressAsync();

                return savedEntries;
            }

            return null;
        }
    }
}
