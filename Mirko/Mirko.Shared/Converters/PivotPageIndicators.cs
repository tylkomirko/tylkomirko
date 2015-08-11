using Mirko.Utils;
using Mirko.ViewModel;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Data;

namespace Mirko.Converters
{
    public class HotTimeSpanStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var timeSpan = (int)value;
            string output = (timeSpan == 1) ? "z ostatniej godziny" : "z ostatnich " + timeSpan + " godzin";

            return output;//" \u25BE";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class HotTimeSpanIndexConverter : IValueConverter
    {
        // timespan - index
        private static Dictionary<int, int> lookupDictionary = new Dictionary<int, int>()
        {
            { 0, 3 }, // just in case something does wrong

            { 1, 0 },
            { 3, 1 },
            { 6, 2 },
            { 12, 3 },
            { 24, 4 },
        };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var timespan = (int)value;
            return lookupDictionary[timespan];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var index = (int)value;

            foreach (var item in lookupDictionary)
                if (item.Value == index)
                    return item.Key;

            return 12;
        }
    }

    public class MyEntriesTypeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var type = (MyEntriesTypeEnum)value;
            return type.GetStringValue();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class MyEntriesTypeIndexConverter : IValueConverter
    {
        private static Dictionary<MyEntriesTypeEnum, int> lookupDictionary = new Dictionary<MyEntriesTypeEnum, int>()
        {
            { MyEntriesTypeEnum.ALL, 0 },
            { MyEntriesTypeEnum.TAGS, 1 },
            { MyEntriesTypeEnum.PEOPLE, 2 },
        };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var type = (MyEntriesTypeEnum)value;
            return lookupDictionary[type];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var index = (int)value;

            foreach (var item in lookupDictionary)
                if (item.Value == index)
                    return item.Key;

            return MyEntriesTypeEnum.ALL;
        }
    }
}
