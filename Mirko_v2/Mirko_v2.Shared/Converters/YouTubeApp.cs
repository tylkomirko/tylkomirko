using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using Mirko_v2.Utils;

namespace Mirko.Converters
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
