using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Mirko.Converters
{
    public class SexToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string str)
        {
            Color c;
            if (value as string == "male")
                c = Color.FromArgb(255, 70, 171, 242);
            else if (value as string == "female")
                c = Color.FromArgb(255, 242, 70, 208);
            else
                c = Color.FromArgb(0, 0, 0, 0);

            return new SolidColorBrush(c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string str)
        {
            throw new NotImplementedException();
        }
    }
}
