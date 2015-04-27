using GalaSoft.MvvmLight.Ioc;
using Mirko_v2;
using Mirko_v2.ViewModel;
using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Mirko.Converters
{
    public class GroupToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string str)
        {
            Color c;
            var settingsVM = SimpleIoc.Default.GetInstance<SettingsViewModel>();

            switch((int)value){
                case 0: // zielonki
                    c = Color.FromArgb(255, 51, 153, 51);
                    break;
                case 1: // pomarancze
                    c = Color.FromArgb(255, 255, 89, 23);
                    break;
                case 2: // bordo
                    c = Color.FromArgb(255, 187, 0, 0);
                    break;
                case 5: // biali
                    if (settingsVM.NightMode)
                        c = Color.FromArgb(255, 255, 255, 255);
                    else
                        c = Color.FromArgb(255, 0, 0, 0);
                    break;
                case 1001: // zbanowani
                    c = Color.FromArgb(255, 153, 153, 153);
                    break;
                case 1002: // usunieci
                    c = Color.FromArgb(255, 153, 153, 153);
                    break;
                case 2001: // niebiescy
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
