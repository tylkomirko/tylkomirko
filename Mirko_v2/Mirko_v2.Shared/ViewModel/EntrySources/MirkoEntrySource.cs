using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WykopAPI.Models;

namespace Mirko_v2.ViewModel
{
    public class MirkoEntrySource : IIncrementalSource<EntryViewModel>
    {
        public void ClearCache()
        {
        }

        public async Task<IEnumerable<EntryViewModel>> GetPagedItems(int pageSize, CancellationToken ct)
        {
            var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();

            if (App.ApiService.IsNetworkAvailable)
            {
                await StatusBarManager.ShowTextAndProgressAsync("Pobieram wpisy...");

                IEnumerable<Entry> newEntries = null;

                if (mainVM.MirkoEntries.Count == 0)
                {
                    newEntries = await App.ApiService.getEntries(0, ct);
                }
                else
                {
                    var lastID = mainVM.MirkoEntries.Last().Data.ID;
                    var tmp = await App.ApiService.getEntries(lastID, 0, ct);
                    if(tmp != null)
                        newEntries = tmp.Skip(1);
                }

                ct.ThrowIfCancellationRequested();

                await StatusBarManager.HideProgressAsync();

                if (newEntries != null)
                {
                    var VMs = new List<EntryViewModel>(newEntries.Count());
                    foreach (var entry in newEntries)
                        VMs.Add(new EntryViewModel(entry));

                    return VMs;
                }
            }
            else
            {
                // offline mode
                if (mainVM.MirkoEntries.Count == 0)
                {
                    await StatusBarManager.ShowTextAndProgressAsync("Wczytuje wpisy...");
                    var savedEntries = await mainVM.ReadCollection("MirkoEntries");
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
