using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using WykopSDK.API.Models;

namespace Mirko_v2.Converters
{
    public class ConversationStatusToBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = (ConversationStatus)value;

            var color = status == ConversationStatus.New ? Colors.White : Colors.Gray;
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
