using System;
using System.Collections.Generic;
using System.Text;

namespace Mirko_v2.Utils
{
    public class PageNavigationEventArgs : EventArgs
    {
        private readonly string _navParam;
        private readonly Type _pageType;
        private readonly bool _force;

        public string NavigationParameter
        {
            get { return this._navParam; }
        }

        public Type PageType
        {
            get { return this._pageType; }
        }

        public bool Force
        {
            get { return this._force; }
        }

        public PageNavigationEventArgs(Type _t, string _p = null, bool _f = false)
        {
            this._pageType = _t;
            this._navParam = _p;
            this._force = _f;
        }
    }

}
