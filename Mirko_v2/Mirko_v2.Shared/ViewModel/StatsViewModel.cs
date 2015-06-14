using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mirko_v2.ViewModel
{
    public class StatsViewModel : ViewModelBase
    {
        private int _callCount = 0;
        public int CallCount
        {
            get { return _callCount; }
            set { Set(() => CallCount, ref _callCount, value); }
        }
    }
}
