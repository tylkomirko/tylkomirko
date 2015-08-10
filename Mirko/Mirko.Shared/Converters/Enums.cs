using Mirko.Utils;
using System;
using Windows.UI.Xaml.Data;

namespace Mirko.Converters
{
    public class YouTubeApp : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var yt = (Mirko.ViewModel.YouTubeApp)value;
            return yt.GetStringValue();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class StartPage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var page = (Mirko.ViewModel.StartPage)value;
            return page.GetStringValue();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
