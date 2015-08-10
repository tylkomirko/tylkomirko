using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Mirko.Utils
{
    public class StringEventArgs : EventArgs
    {
        private readonly string _string;

        public StringEventArgs(string s)
        {
            this._string = s;
        }

        public string String
        {
            get { return this._string; }
        }
    }
}
