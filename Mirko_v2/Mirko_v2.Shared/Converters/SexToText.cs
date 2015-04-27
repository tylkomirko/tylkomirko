using System;
using Windows.UI.Xaml.Data;

namespace Mirko.Converters
{
    public class SexToText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var sex = value as string;
            if (sex == "male" || sex == "female")
                return "\u2022";
            else
                return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
