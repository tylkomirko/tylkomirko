using System;
using System.Linq;
using Windows.UI.Xaml.Data;
using WykopSDK.API.Models;

namespace Mirko_v2.Converters
{
    public class NSFWToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";

            var embed = value as Embed;
            if (embed.NSFW)
                return "+18";
            else
                return embed.URL.Split('/').Last();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
