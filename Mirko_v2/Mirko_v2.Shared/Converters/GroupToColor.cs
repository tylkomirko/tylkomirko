using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.ViewModel;
using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using WykopAPI.Models;

namespace Mirko_v2.Converters
{
    public class GroupToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string str)
        {
            Color c;
            var settingsVM = SimpleIoc.Default.GetInstance<SettingsViewModel>();

            switch((UserGroup)value){
                case UserGroup.Green: // zielonki
                    c = Color.FromArgb(255, 51, 153, 51);
                    break;
                case UserGroup.Orange: // pomarancze
                    c = Color.FromArgb(255, 255, 89, 23);
                    break;
                case UserGroup.Maroon: // bordo
                    c = Color.FromArgb(255, 187, 0, 0);
                    break;
                case UserGroup.Admin: // biali
                    if (settingsVM.SelectedTheme == Windows.UI.Xaml.ElementTheme.Dark)
                        c = Color.FromArgb(255, 255, 255, 255);
                    else
                        c = Color.FromArgb(255, 0, 0, 0);
                    break;
                case UserGroup.Banned: // zbanowani
                    c = Color.FromArgb(255, 153, 153, 153);
                    break;
                case UserGroup.Deleted: // usunieci
                    c = Color.FromArgb(255, 153, 153, 153);
                    break;
                case UserGroup.Client: // niebiescy
                    c = Color.FromArgb(255, 63, 111, 160);
                    break;
            }

            return new SolidColorBrush(c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string str)
        {
            throw new NotImplementedException();
        }
    }
}
