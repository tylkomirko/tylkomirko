using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Mirko_v2.Converters
{
    public class InvertVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var visibility = (Visibility)value;
            if (visibility == Visibility.Collapsed)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
