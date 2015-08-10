using System;
using Windows.UI.Xaml.Data;
using WykopSDK.API.Models;

namespace Mirko.Converters
{
    public class SexToText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var sex = (UserSex)value;
            if (sex == UserSex.None)
                return "";
            else
                return "\u2022";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
