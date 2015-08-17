using Mirko.ViewModel;
using System;
using Windows.UI.Xaml.Data;

namespace Mirko.Converters
{
    public class ObjectToEntryVM : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var vm = value as EntryViewModel;
            return vm;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var vm = value as EntryViewModel;
            return vm;
        }
    }
}
