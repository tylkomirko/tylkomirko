using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using WykopAPI.Models;

namespace Mirko_v2.Converters
{
    public class ConversationStatusToBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = (ConversationStatus)value;
            if (status == ConversationStatus.New)
                return new SolidColorBrush(Colors.White);
            else
                return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
