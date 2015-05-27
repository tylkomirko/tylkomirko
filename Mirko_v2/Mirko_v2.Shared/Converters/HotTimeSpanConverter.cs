﻿using System;
using Windows.UI.Xaml.Data;

namespace Mirko.Converters
{
    public class HotTimeSpanStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var timeSpan = (int)value;
            string output = (timeSpan == 1) ? "z ostatniej godziny" : "z ostatnich " + timeSpan + " godzin";

            return output + " \u25BE";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class HotTimeSpanIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var timespan = (int)value;

            if (timespan == 1)
                return 0;
            else if (timespan == 3)
                return 1;
            else if (timespan == 6)
                return 2;
            else if (timespan == 12)
                return 3;
            else
                return 3;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var index = (int)value;

            if (index == 0)
                return 1;
            else if (index == 1)
                return 3;
            else if (index == 2)
                return 6;
            else if (index == 3)
                return 12;
            else
                return 12;
        }
    }
}
