using GalaSoft.MvvmLight.Ioc;
using Mirko.ViewModel;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Mirko.Converters
{
    public class ProfileAppBar : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var userInfo = SimpleIoc.Default.GetInstance<SettingsViewModel>().UserInfo;
            if (userInfo == null)
                return Visibility.Collapsed;

            var username = value as string;
            return username != userInfo.UserName ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
