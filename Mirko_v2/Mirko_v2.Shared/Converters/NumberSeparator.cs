using System;
using Windows.UI.Xaml.Data;

namespace Mirko.Converters
{
    public class NumberSeparator : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int number = (int)value;
            var thousands = number / 1000;
            var leftover = number % 1000;

            if (thousands >= 1)
                return thousands + " " + leftover;
            else
                return number;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
