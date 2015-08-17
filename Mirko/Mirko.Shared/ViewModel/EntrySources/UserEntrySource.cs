using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Mirko.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mirko.ViewModel
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
                await StatusBarManager.ShowTextAndProgressAsync("Pobieram wpisy...");
                var newEntries = await App.ApiService.GetUserEntries(currentUserName, pageIndex++);
                await StatusBarManager.HideProgressAsync();

                if (newEntries != null)
                {
                    if (newEntries.Count == 0)
                    {
                        DispatcherHelper.CheckBeginInvokeOnUI(() => profileVM.Entries.HasMoreItems = false);
                        if(profileVM.Entries.Count == 0)
                            DispatcherHelper.CheckBeginInvokeOnUI(() => profileVM.Entries.HasNoItems = true);
                    }

                    return newEntries.Select(x => new EntryViewModel(x));
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
