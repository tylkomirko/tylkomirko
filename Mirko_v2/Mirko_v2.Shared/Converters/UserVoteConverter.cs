using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Mirko.Converters
{
    public class UserVoteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string str)
        {
            if ((int)value == 1)
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string str)
        {
            throw new NotImplementedException();
        }
    }
}
