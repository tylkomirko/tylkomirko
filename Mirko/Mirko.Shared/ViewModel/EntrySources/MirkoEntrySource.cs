﻿using GalaSoft.MvvmLight.Ioc;
using Mirko.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WykopSDK.API.Models;

namespace Mirko.ViewModel
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
                    newEntries = await App.ApiService.GetEntries(0, ct);
                }
                else
                {
                    var lastID = mainVM.MirkoEntries.Last().Data.ID;
                    var tmp = await App.ApiService.GetEntries(0, ct, lastID);
                    if(tmp != null)
                        newEntries = tmp.Skip(1);
                }
                
                await StatusBarManager.HideProgressAsync();

                if (newEntries == null)
                    return null;

                ct.ThrowIfCancellationRequested();

                return newEntries.Select(x => new EntryViewModel(x));
            }
            else if (mainVM.MirkoEntries.Count == 0)
            {
                // offline mode
                await StatusBarManager.ShowTextAndProgressAsync("Wczytuje wpisy...");
                var savedEntries = await mainVM.ReadCollection("MirkoEntries");
                await StatusBarManager.HideProgressAsync();

                return savedEntries;
            }

            return null;
        }
    }
}
