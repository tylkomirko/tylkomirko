using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Mirko.Converters
{
    public class CountToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string str)
        {
            var count = (uint?)value;
            if (count > 0)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string str)
        {
            throw new NotImplementedException();
        }
    }
}
