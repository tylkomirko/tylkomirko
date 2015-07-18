using System;
using Windows.UI.Xaml.Data;

namespace Mirko_v2.Converters
{
    public class NewEntryPageTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var id = (uint)value;

            return id == 0 ? "nowy wpis" : "nowy komentarz";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
