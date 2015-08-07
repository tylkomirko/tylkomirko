using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Mirko_v2.Utils;
using System;
using System.Threading.Tasks;
using WykopSDK.API;

namespace Mirko_v2.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        public string Username { get; set; }
        public string Password { get; set; }

        private RelayCommand _loginCommand = null;
        public RelayCommand LoginCommand
        {
            get { return _loginCommand ?? (_loginCommand = new RelayCommand(ExecuteLoginCommand)); }
        }

        private async void ExecuteLoginCommand()
        {
            await StatusBarManager.ShowTextAndProgressAsync("Logowanie...");

            var serverResponse = new Action<string>(response =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    var msg = new NotificationMessage<string>("Serwer odpowiedział: " + response, "LoginPageFlyout");
                    Messenger.Default.Send(msg);

                    StatusBarManager.HideProgress();
                });
            });

            var error = new Action<string>(err =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    var msg = new NotificationMessage<string>(err, "LoginPageFlyout");
                    Messenger.Default.Send(msg);

                    StatusBarManager.HideProgress();
                });
            });

            var success = new Action<UserInfo>(userInfo =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    App.ApiService.UserInfo = userInfo;
                    SimpleIoc.Default.GetInstance<SettingsViewModel>().UserInfo = userInfo;

                    // navigate off to somewhere
                    SimpleIoc.Default.GetInstance<NavigationService>().NavigateTo("PivotPage");
                    Messenger.Default.Send(new NotificationMessage("Login"));

                    StatusBarManager.HideProgress();
                });
            });

            await Task.Run(async () =>
            await App.WWWService.ConnectAccount(
                Username, Password,
                App.ApiService.APPKEY, App.ApiService.SECRETKEY,
                serverResponseCallback: serverResponse,
                errorCallback: error,
                successCallback: success));
        }
    }
}
