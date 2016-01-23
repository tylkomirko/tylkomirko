using GalaSoft.MvvmLight.Ioc;
using Mirko.ViewModel;
using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Mirko.Converters
{
    public class AuthorIndicator : IValueConverter
    {
        private static SettingsViewModel Settings = null;

        private string AppendIfLight(string input)
        {
            if (Settings.SelectedTheme == ElementTheme.Light)
                return $"{input}Light";
            else
                return input;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(Settings == null)
                Settings = SimpleIoc.Default.GetInstance<SettingsViewModel>();

            var VM = value as CommentViewModel;
            if (VM == null || targetType != typeof(Brush)) return null;

            string myUsername = App.ApiService.UserInfo?.UserName ?? null;
            string currentAuthor = VM.Data.AuthorName;
            string magicUsernameString = $"@<a href=\"@{myUsername}\">{myUsername}</a>";

            if (myUsername != null && VM.Data.Text.Contains(magicUsernameString))
                return App.Current.Resources[AppendIfLight("CommentDirectedFill")] as Brush;
            else if (currentAuthor == VM.RootEntryAuthor)
                return App.Current.Resources[AppendIfLight("AuthorCommentFill")] as Brush;
            else
                return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
