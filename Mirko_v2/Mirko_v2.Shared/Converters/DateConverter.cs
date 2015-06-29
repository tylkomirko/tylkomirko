using Mirko_v2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Mirko.Converters
{
    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string str)
        {
            if (value == null)
                return "";

            DateTime now = DateTime.UtcNow;
            DateTime entryTime = (DateTime)value;
            entryTime = entryTime.Subtract(App.OffsetUTCInPoland);
            string result = string.Empty;

            var diff = now - entryTime;
            if (diff.Days > 0)
            {
                if (diff.Days == 1)
                {
                    result = "1 dzień temu";
                }
                else if (diff.Days > 31)
                {
                    var months = diff.Days / 31;
                    if (months == 1)
                        result = "1 miesiąc temu";
                    else if (months == 2 || months == 3 || months == 4)
                        result = months + " miesiące temu";
                    else
                        result = months + " miesięcy temu";
                }
                else
                {
                    result = diff.Days + " dni temu";
                }
            }
            else if (diff.Hours > 0)
            {
                result = diff.Hours + " godz. temu";
            }
            else if (diff.Minutes > 0)
            {
                result = diff.Minutes + " min. temu";
            }
            else if (diff.Seconds > 0)
            {
                if (diff.Seconds > 30)
                    result = diff.Seconds + " sek. temu";
                else
                    result = "chwilę temu";
            }
            else
            {
                result = "chwilę temu";
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string str)
        {
            throw new NotImplementedException();
        }
    }
}
