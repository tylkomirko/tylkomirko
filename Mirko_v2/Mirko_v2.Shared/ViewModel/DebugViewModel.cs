using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Background;

namespace Mirko_v2.ViewModel
{
    public class DebugViewModel : ViewModelBase
    {
        public ObservableCollectionEx<string> RegisteredBackgroundTasks
        {
            get
            {
                var list = new ObservableCollectionEx<string>();
                foreach (var cur in BackgroundTaskRegistration.AllTasks)
                    list.Add(cur.Value.Name);
                return list;
            }
        }
    }
}
