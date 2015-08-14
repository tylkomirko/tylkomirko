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
                return string.Format("{0} {1}", thousands, leftover);
            else
                return number.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
