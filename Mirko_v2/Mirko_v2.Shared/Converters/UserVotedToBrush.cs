using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Mirko_v2.Converters
{
    public class UserVotedToBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var user_voted = (bool)value;
            if (user_voted)
                return new SolidColorBrush(Colors.DarkGreen);
            else
                return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
