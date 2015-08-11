using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Mirko.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mirko.ViewModel
{
    public class FavEntrySource : IIncrementalSource<EntryViewModel>
    {
        public void ClearCache()
        {
        }

        public async Task<IEnumerable<EntryViewModel>> GetPagedItems(int pageSize, CancellationToken ct)
        {
            var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();

            ct.ThrowIfCancellationRequested();

            if (App.ApiService.IsNetworkAvailable && mainVM.FavEntries.Count == 0)
            {
                await StatusBarManager.ShowTextAndProgressAsync("Pobieram wpisy...");
                var newEntries = await App.ApiService.GetFavourites(ct);
                await StatusBarManager.HideProgressAsync();

                if (newEntries != null)
                {
                    if (newEntries.Count() == 0)
                    {
                        await DispatcherHelper.RunAsync(() =>
                        {
                            mainVM.FavEntries.HasNoItems = true;
                            mainVM.FavEntries.HasMoreItems = false;
                        });
                    }

                    return newEntries.Select(x => new EntryViewModel(x));
                }
                else
                {
                    await DispatcherHelper.RunAsync(() =>
                    {
                        mainVM.FavEntries.HasNoItems = true;
                        mainVM.FavEntries.HasMoreItems = false;
                    });
                }
            }
            else if (!App.ApiService.IsNetworkAvailable && mainVM.FavEntries.Count == 0)
            {
                // offline mode
                await StatusBarManager.ShowTextAndProgressAsync("Wczytuje wpisy...");
                var savedEntries = await mainVM.ReadCollection("FavEntries");
                await StatusBarManager.HideProgressAsync();

                return savedEntries;
            }

            return null;
        }
    }
}
