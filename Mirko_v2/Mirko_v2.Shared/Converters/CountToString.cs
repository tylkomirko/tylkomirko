using System;
using Windows.UI.Xaml.Data;

namespace Mirko.Converters
{
    public class CountToString: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string str = string.Empty;
            if (value is uint && (uint)value > 0)
                str = ((uint)value).ToString();
            else if (value is int && (int)value > 0)
                str = ((int)value).ToString();

            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
