using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Mirko.Utils
{
    public static class CommandBarUtils
    {
        public static void MakeButtonVisible(this CommandBar c, string tag)
        {
            foreach(var command in c.PrimaryCommands)
            {
                var button = command as FrameworkElement;
                if (button != null)
                {
                    var t = button.Tag as string;
                    if (!string.IsNullOrEmpty(t) && t == tag)
                        button.Visibility = Visibility.Visible;
                }
            }
        }

        public static void MakeButtonInvisible(this CommandBar c, string tag)
        {
            foreach (var command in c.PrimaryCommands)
            {
                var button = command as FrameworkElement;
                if (button != null)
                {
                    var t = button.Tag as string;
                    if (!string.IsNullOrEmpty(t) && t == tag)
                        button.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
