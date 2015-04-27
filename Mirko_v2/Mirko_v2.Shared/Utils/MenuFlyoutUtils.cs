using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Mirko_v2.Common
{
    public static class MenuFlyoutUtils
    {
        public static void MakeItemVisible(ref MenuFlyout mf, string tag)
        {
            var items = mf.Items;
            foreach (var i in items)
            {
                if (i.Tag != null && i.Tag.ToString() == tag)
                {
                    i.Visibility = Visibility.Visible;
                    break;
                }
            }
        }

        public static void MakeItemInvisible(ref MenuFlyout mf, string tag)
        {
            var items = mf.Items;
            foreach (var i in items)
            {
                if (i.Tag != null && i.Tag.ToString() == tag)
                {
                    i.Visibility = Visibility.Collapsed;
                    break;
                }
            }
        }
    }
}
