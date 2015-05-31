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
        public void ClearCache()
        {
        }

        public async Task<IEnumerable<EntryViewModel>> GetPagedItems(int pageSize, CancellationToken ct)
        {
            var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();

            if (App.ApiService.IsNetworkAvailable && mainVM.FavEntries.Count == 0)
            {
                await StatusBarManager.ShowTextAndProgress("Pobieram wpisy...");
                var newEntries = await App.ApiService.getFavourites();
                await StatusBarManager.HideProgress();

                if (newEntries != null)
                {
                    if (newEntries.Count() == 0)
                    {
                        mainVM.FavEntries.HasNoItems = true;
                        mainVM.FavEntries.HasMoreItems = false;
                    }

                    var VMs = new List<EntryViewModel>(newEntries.Count);
                    foreach (var entry in newEntries)
                        VMs.Add(new EntryViewModel(entry));

                    return VMs;
                }
            }
            else if (!App.ApiService.IsNetworkAvailable)
            {
                // offline mode
                if (mainVM.FavEntries.Count == 0)
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

            return null;
        }
    }
}
