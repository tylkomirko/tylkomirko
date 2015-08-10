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

                    var VMs = new List<EntryViewModel>(newEntries.Count);
                    foreach (var entry in newEntries)
                        VMs.Add(new EntryViewModel(entry));

                    return VMs;
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
            else if (!App.ApiService.IsNetworkAvailable)
            {
                // offline mode
                if (mainVM.FavEntries.Count == 0)
                {
                    await StatusBarManager.ShowTextAndProgressAsync("Wczytuje wpisy...");
                    var savedEntries = await mainVM.ReadCollection("FavEntries");
                    await StatusBarManager.HideProgressAsync();

                    return savedEntries;
                }
                else
                {
                    return null;
                }
            }

            return null;
        }
    }
}
