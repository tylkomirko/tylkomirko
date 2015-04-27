using System;
using Windows.UI.Xaml.Data;

namespace Mirko.Converters
{
    public class CountToString: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var count = (int)value;
            if (count > 0)
                return count.ToString();
            else
                return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
