using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Mirko_v2.Converters
{
    public class CountToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string str)
        {
            if (value is uint)
                return (uint)value > 0 ? Visibility.Visible : Visibility.Collapsed;
            else
                return (int)value > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string str)
        {
            throw new NotImplementedException();
        }
    }

    public class CountToVisibilityInv : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string str)
        {
            if (value is uint)
                return (uint)value > 0 ? Visibility.Collapsed : Visibility.Visible;
            else
                return (int)value > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string str)
        {
            throw new NotImplementedException();
        }
    }
}
