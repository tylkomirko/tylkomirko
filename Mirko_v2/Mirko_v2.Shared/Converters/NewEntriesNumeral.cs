using System;
using Windows.UI.Xaml.Data;

namespace Mirko_v2.Converters
{
    public class NewEntriesNumeral : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int count = (int)value;

            if (count == 1)
                return "1 nowy wpis";
            else if (count >= 2 && count <= 4)
                return count + " nowe wpisy";
            else
                return count + " nowych wpisów";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
