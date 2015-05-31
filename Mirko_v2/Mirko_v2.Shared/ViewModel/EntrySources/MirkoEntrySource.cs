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
        private int pageIndex = 0;

        public void ClearCache()
        {
            pageIndex = 0;
        }

        public async Task<IEnumerable<EntryViewModel>> GetPagedItems(int pageSize, CancellationToken ct)
        {
            var entriesToReturn = new List<Entry>(45);
            var mainVM = SimpleIoc.Default.GetInstance<MainViewModel>();

            if (App.ApiService.IsNetworkAvailable)
            {
                await StatusBarManager.ShowTextAndProgress("Pobieram wpisy...");

                IEnumerable<Entry> newEntries = null;

                if (pageIndex == 0)
                {
                    newEntries = await App.ApiService.getEntries(pageIndex++);
                }
                else
                {
                    var lastID = mainVM.MirkoEntries.Last().Data.ID;
                    var tmp = await App.ApiService.getEntries(lastID, 0);
                    if(tmp != null)
                        newEntries = tmp.Skip(1);
                }

                if (newEntries != null)
                    entriesToReturn.AddRange(newEntries);

                await StatusBarManager.HideProgress();
            }
            else
            {
                // offline mode
                if (mainVM.MirkoEntries.Count == 0)
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

            var VMs = new List<EntryViewModel>(entriesToReturn.Count);
            foreach (var entry in entriesToReturn)
                VMs.Add(new EntryViewModel(entry));

            return VMs;
        }
    }
}
