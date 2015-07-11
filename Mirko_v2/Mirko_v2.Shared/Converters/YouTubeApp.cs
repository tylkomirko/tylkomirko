using Mirko_v2.Utils;
using System;
using Windows.UI.Xaml.Data;

namespace Mirko_v2.Converters
{
    public class YouTubeApp : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var yt = (Mirko_v2.ViewModel.YouTubeApp)value;
            return yt.GetStringValue();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
