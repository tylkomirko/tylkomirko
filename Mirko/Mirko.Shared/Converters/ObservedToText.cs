using System;
using Windows.UI.Xaml.Data;

namespace Mirko.Converters
{
    public class ObservedToText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? "przestań obserwować" : "obserwuj";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
