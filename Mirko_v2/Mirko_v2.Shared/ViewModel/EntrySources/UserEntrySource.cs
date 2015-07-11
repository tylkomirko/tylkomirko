using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WykopAPI.Models;
using GalaSoft.MvvmLight.Threading;

namespace Mirko_v2.ViewModel
{
    public class UserEntrySource : IIncrementalSource<EntryViewModel>
    {
        private int pageIndex = 1;

        public void ClearCache()
        {
            pageIndex = 1;
        }

        public async Task<IEnumerable<EntryViewModel>> GetPagedItems(int pageSize, CancellationToken ct)
        {
            var profileVM = SimpleIoc.Default.GetInstance<ProfilesViewModel>().CurrentProfile;
            var currentUserName = profileVM.Data.Login;

            if (App.ApiService.IsNetworkAvailable)
            {
                await StatusBarManager.ShowTextAndProgress("Pobieram wpisy...");
                var newEntries = await App.ApiService.getUserEntries(currentUserName, pageIndex++);
                await StatusBarManager.HideProgress();

                if (newEntries != null)
                {
                    if (newEntries.Count == 0)
                    {
                        DispatcherHelper.CheckBeginInvokeOnUI(() => profileVM.Entries.HasMoreItems = false);
                        if(profileVM.Entries.Count == 0)
                            DispatcherHelper.CheckBeginInvokeOnUI(() => profileVM.Entries.HasNoItems = true);
                    }

                    var VMs = new List<EntryViewModel>(newEntries.Count());
                    foreach (var entry in newEntries)
                        VMs.Add(new EntryViewModel(entry));

                    return VMs;
                }
            }
            else
            {
                /*
                // offline mode
                if (profileVM.UserEntries.Count == 0)
                {
                    await StatusBarManager.ShowTextAndProgress("Wczytuje wpisy...");
                    var savedEntries = await profileVM.ReadCollection("MirkoEntries");
                    await StatusBarManager.HideProgress();

                    return savedEntries;
                }
                else
                {
                    return null;
                }*/
            }

            return null;
        }
    }
}
