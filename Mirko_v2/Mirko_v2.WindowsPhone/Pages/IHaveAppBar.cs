using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Mirko_v2.Pages
{
    interface IHaveAppBar
    {
        CommandBar CreateCommandBar();
    }
}
