using Microsoft.Xaml.Interactivity;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Mirko_v2.Utils
{
    public class CloseFlyoutAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            var flyout = sender as FlyoutBase;
            if (flyout == null)
                throw new ArgumentException("CloseFlyoutAction can be used only with Flyout");

            flyout.Hide();

            return null;
        }
    } 
}
