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
    public class TaggedEntrySource : IIncrementalSource<EntryViewModel>
    {
        private int pageIndex = 1;

        public void ClearCache()
        {
            pageIndex = 1;
        }

        public async Task<IEnumerable<EntryViewModel>> GetPagedItems(int pageSize, CancellationToken ct)
        {
            var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();

            if (mainVM.SelectedHashtag == null || string.IsNullOrEmpty(mainVM.SelectedHashtag.Hashtag))
                return null;

            var tag = mainVM.SelectedHashtag.Hashtag;

            ct.ThrowIfCancellationRequested();

            List<Entry> newEntries = null;
            if (App.ApiService.IsNetworkAvailable)
            {
                await StatusBarManager.ShowTextAndProgressAsync("Pobieram wpisy...");

                var newEntriesContainer = await App.ApiService.GetTaggedEntries(tag, pageIndex++, ct);

                await StatusBarManager.HideProgressAsync();

                if (newEntriesContainer == null)
                    return null;

                newEntries = newEntriesContainer.Entries;
                if (newEntries.Count > 0)
                    DispatcherHelper.CheckBeginInvokeOnUI(() => mainVM.SelectedHashtag = newEntriesContainer.Meta); // is this needed?
            }

            ct.ThrowIfCancellationRequested();

            if (newEntries.Count == 0 && mainVM.TaggedEntries.Count == 0)
                await DispatcherHelper.RunAsync(() => mainVM.TaggedEntries.HasNoItems = true);

            return newEntries.Select(x => new EntryViewModel(x));
        }
    }
}
