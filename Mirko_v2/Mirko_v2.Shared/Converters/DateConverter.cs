using Mirko_v2;
using System;
using Windows.UI.Xaml.Data;

namespace Mirko_v2.Converters
{
    public static class EntryTimeConverter
    {
        public static string Convert(DateTime entryTime)
        {
            DateTime now = DateTime.UtcNow;
            entryTime = entryTime.Subtract(App.OffsetUTCInPoland);
            string result = string.Empty;

            var diff = now - entryTime;
            if (diff.Days > 0)
            {
                if (diff.Days == 1)
                {
                    result = "1 dzień";
                }
                else if (diff.Days > 31)
                {
                    var months = diff.Days / 31;
                    if (months == 1)
                        result = "1 miesiąc";
                    else if (months == 2 || months == 3 || months == 4)
                        result = months + " miesiące";
                    else
                    {
                        if (months > 12)
                        {
                            var years = months / 12;
                            var leftoverMonths = months % 12;

                            if (years == 1)
                                result = "1 rok";
                            else if (years == 2 || years == 3 || years == 4)
                                result = years + " lata";
                            else
                                result = years + " lat";

                            if (leftoverMonths > 0)
                                result += " " + leftoverMonths + " mies.";
                        }
                        else
                        {
                            result = months + " miesięcy";
                        }
                    }
                }
                else
                {
                    result = diff.Days + " dni";
                }
            }
            else if (diff.Hours > 0)
            {
                result = diff.Hours + " godz.";
            }
            else if (diff.Minutes > 0)
            {
                result = diff.Minutes + " min.";
            }
            else if (diff.Seconds > 0)
            {
                if (diff.Seconds > 30)
                    result = diff.Seconds + " sek.";
                else
                    result = "chwilę";
            }
            else
            {
                result = "chwilę";
            }

            return result;
        }
    }

    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string str)
        {
            if (value == null)
                return "";

            DateTime entryTime = (DateTime)value;
            return EntryTimeConverter.Convert(entryTime);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string str)
        {
            throw new NotImplementedException();
        }
    }

    public class LongDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return "";

            DateTime entryTime = (DateTime)value;
            return EntryTimeConverter.Convert(entryTime) + " temu";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
