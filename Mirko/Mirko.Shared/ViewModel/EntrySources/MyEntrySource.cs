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
    public enum MyEntriesTypeEnum
    {
        [StringValue("wszystko")]
        ALL,
        [StringValue("tagi")]
        TAGS,
        [StringValue("ludzie")]
        PEOPLE,
    };

    public class MyEntrySource : IIncrementalSource<EntryViewModel>
    {
        private int pageIndex = 1;

        public void ClearCache()
        {
            pageIndex = 1;
        }

        private bool? enumToBool(MyEntriesTypeEnum e)
        {
            if (e == MyEntriesTypeEnum.ALL)
                return null;
            else if (e == MyEntriesTypeEnum.TAGS)
                return true;
            else
                return false;
        }

        public async Task<IEnumerable<EntryViewModel>> GetPagedItems(int pageSize, CancellationToken ct)
        {
            var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();

            ct.ThrowIfCancellationRequested();

            IEnumerable<Entry> newEntries = null;
            if (App.ApiService.IsNetworkAvailable)
            {
                await StatusBarManager.ShowTextAndProgressAsync("Pobieram wpisy...");

                newEntries = await App.ApiService.GetMyEntries(pageIndex++, ct, getTags: enumToBool(mainVM.MyEntriesType));

                if (newEntries == null || newEntries.Count() == 0)
                {
                    await DispatcherHelper.RunAsync(() => mainVM.MyEntries.HasMoreItems = false);
                    if (mainVM.MyEntries.Count == 0 && pageIndex == 2)
                        await DispatcherHelper.RunAsync(() => mainVM.MyEntries.HasNoItems = true);
                }

                await StatusBarManager.HideProgressAsync();
            }
            else if (mainVM.MyEntries.Count == 0)
            {
                // offline mode
                await StatusBarManager.ShowTextAndProgressAsync("Wczytuje wpisy...");
                var savedEntries = await mainVM.ReadCollection("MyEntries");
                await StatusBarManager.HideProgressAsync();

                return savedEntries;
            }

            var VMs = new List<EntryViewModel>(newEntries.Count());
            foreach (var entry in newEntries)
                VMs.Add(new EntryViewModel(entry));

            return VMs;
        }
    }
}

