using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Mirko.Converters
{
    public class PMDirectionToVisibility: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var direction = value as string;
            if (direction == "received")
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
