using GalaSoft.MvvmLight.Ioc;
using Mirko.ViewModel;
using System;
using Windows.UI.Xaml.Data;

namespace Mirko.Converters
{
    public class FontSizeScaler : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var input = (double)value;
            var settingsVM = SimpleIoc.Default.GetInstance<SettingsViewModel>();
            var scale = settingsVM.FontScaleFactor;

            return input * scale;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
