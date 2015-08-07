using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using WykopSDK.API.Models;

namespace Mirko_v2.Converters
{
    public class SexToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string str)
        {
            UserSex s = (UserSex)value;
            Color c;
            if (s == UserSex.Male)
                c = Color.FromArgb(255, 70, 171, 242);
            else if (s == UserSex.Female)
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
