using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using WykopAPI.Models;

namespace Mirko_v2.Converters
{
    public class CommentListToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string str)
        {
            var list = value as List<EntryComment>;
            if (list != null)
            {
                if (list.Count() > 0)
                    return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string str)
        {
            throw new NotImplementedException();
        }
    }
}
