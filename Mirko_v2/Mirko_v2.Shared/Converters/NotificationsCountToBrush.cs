using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Mirko.Converters
{
    public class NotificationsCountToBrush: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var count = (uint)value;

            if (count > 0)
                return Application.Current.Resources["NotificationsForeground"] as SolidColorBrush;
            else
                return Application.Current.Resources["NoNotificationsForeground"] as SolidColorBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
