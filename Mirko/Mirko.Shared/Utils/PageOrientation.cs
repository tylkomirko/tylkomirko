using System;
using Windows.Graphics.Display;
using Windows.UI.Xaml;

namespace Mirko.Utils
{
    public class PageOrientation
    {
        public static readonly DependencyProperty SupportedOrientationsProperty =
            DependencyProperty.RegisterAttached("SupportedOrientations",
            typeof(String), typeof(PageOrientation),
            new PropertyMetadata(null, OnSupportedOrientationsChanged));

        public static void SetSupportedOrientations(UIElement element, object value)
        {
            element.SetValue(SupportedOrientationsProperty, value);
        }

        public static object GetSupportedOrientations(UIElement element)
        {
            return element.GetValue(SupportedOrientationsProperty);
        }

        private static void OnSupportedOrientationsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DisplayOrientations supportedOrientations = (DisplayOrientations)Enum.Parse(typeof(DisplayOrientations), (string)args.NewValue);

            DisplayInformation.AutoRotationPreferences = supportedOrientations;
        }
    }
}
