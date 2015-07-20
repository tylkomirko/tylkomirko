using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Mirko_v2.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Windows.Security.Credentials;
using WykopAPI;

namespace Mirko_v2.ViewModel
{
    public class LoginShowFlyoutMessage
    {
        public enum FlyoutTypeEnum
        {
            Error,
            Permissions,
        };

        public FlyoutTypeEnum FlyoutType;
        public string ErrorMessage;

        public string PermissionsTitle;
        public List<string> Permissions;
    }

    public class LoginViewModel : ViewModelBase
    {
        private HttpClient client = null;
        private HttpClientHandler handler = null;
        private string lastURL = null;
        private List<string> permissionNames = null;
        private string magicalToken = null;
        private string endURL = "http://www.wykop.pl/user/ConnectSuccess/";
        private const string RESOURCE_NAME = "Credentials";

        private class JsonReply
        {
            public string appkey { get; set; }
            public string login { get; set; }
            public string token { get; set; }
            public string sign { get; set; }
        }

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

            string startURL = "https://a.wykop.pl/user/connect/appkey," + App.ApiService.APPKEY;
            string redirectURL = "https://www.wykop.pl/user/ConnectSuccess";

            var encodedRedirectURL = WebUtility.UrlEncode(Cryptography.EncodeBase64(redirectURL));
            var secure = App.ApiService.SECRETKEY + redirectURL;

            startURL += ",redirect," + encodedRedirectURL + ",secure," + Cryptography.EncodeMD5(secure);

            var post = new List<KeyValuePair<string, string>>();
            post.Add(new KeyValuePair<string, string>("login[login]", Username));
            post.Add(new KeyValuePair<string, string>("login[password]", Password));

            this.handler = new HttpClientHandler() { AllowAutoRedirect = true };
            this.client = new HttpClient(new RetryHandler(handler));
            this.permissionNames = new List<string>();

            using (var content = new FormUrlEncodedContent(post))
            using (var response = await client.PostAsync(startURL, content))
            {
                if (!response.IsSuccessStatusCode)
                {
                    Messenger.Default.Send<LoginShowFlyoutMessage>(new LoginShowFlyoutMessage()
                    {
                        FlyoutType = LoginShowFlyoutMessage.FlyoutTypeEnum.Error,
                        ErrorMessage = "Serwer odpowiedział: " + response.StatusCode.ToString(),
                    });

                    await StatusBarManager.HideProgressAsync();
                    return;
                }

                this.lastURL = response.RequestMessage.RequestUri.ToString();
                if (lastURL.StartsWith(endURL))
                {
                    processFinalURL(this.lastURL);
                }
                else
                {
                    var html = await response.Content.ReadAsStringAsync();
                    var index = html.IndexOf(@"<div class=""error"">");
                    if (index != -1) // error
                    {
                        index += 19;
                        var lastIndex = html.IndexOf("</div>", index);

                        Messenger.Default.Send<LoginShowFlyoutMessage>(new LoginShowFlyoutMessage()
                        {
                            FlyoutType = LoginShowFlyoutMessage.FlyoutTypeEnum.Error,
                            ErrorMessage = html.Substring(index, lastIndex - index).Trim(),
                        });

                        await StatusBarManager.HideProgressAsync();
                        return;
                    }
                    else
                    {
                        index = html.IndexOf("<h3>");
                        if (index != -1)
                        {
                            index += 4;
                            var nextIndex = html.IndexOf("</h3>", index);
                            var title = html.Substring(index, nextIndex - index).Trim().Replace("<b>", "").Replace("</b>", "");

                            var items = new List<string>();
                            while (true)
                            {
                                index = html.IndexOf(@"connect[permissions][]", index);
                                if (index == -1) break;

                                // get permission value
                                index += 22;
                                index = html.IndexOf(@"value=""", index);
                                index += 7;
                                nextIndex = html.IndexOf('"', index);

                                var permissionName = html.Substring(index, nextIndex - index).Trim();
                                this.permissionNames.Add(permissionName);

                                // get permission description
                                index = html.IndexOf(@"checked=""checked"">", nextIndex);
                                index += 18;
                                nextIndex = html.IndexOf(@"</label>", index);

                                var txt = html.Substring(index, nextIndex - index).Trim();
                                items.Add(txt);
                                index = nextIndex;
                            }

                            // get token
                            index = html.IndexOf(@"name=""__token""", nextIndex);
                            index += 14;
                            index = html.IndexOf(@"value=""", index);
                            index += 7;
                            nextIndex = html.IndexOf('"', index);
                            this.magicalToken = html.Substring(index, nextIndex - index).Trim();

                            Messenger.Default.Send<LoginShowFlyoutMessage>(new LoginShowFlyoutMessage()
                            {
                                FlyoutType = LoginShowFlyoutMessage.FlyoutTypeEnum.Permissions,
                                Permissions = new List<string>(items),
                                PermissionsTitle = title,
                            });
                        }
                    }
                }
            }

            await StatusBarManager.HideProgressAsync();
        }

        private void processFinalURL(string url)
        {
            var index = url.IndexOf("connectData=") + 12;
            var base64Encoded = url.Substring(index);
            var json = Cryptography.DecodeBase64(base64Encoded);

            var result = JsonConvert.DeserializeObject<JsonReply>(json);
            var userInfo = new WykopAPI.UserInfo();
            if (result.appkey == App.ApiService.APPKEY)
            {
                userInfo.UserName = result.login;
                userInfo.AccountKey = result.token;
                userInfo.UserKey = "maciejkochamcie";

                App.ApiService.UserInfo = userInfo;
                SimpleIoc.Default.GetInstance<SettingsViewModel>().UserInfo = userInfo;

                // clean up
                this.client.Dispose();
                this.handler.Dispose();
                this.permissionNames = null;
                this.magicalToken = null;

                SaveCredentials();

                // navigate off to somewhere
                SimpleIoc.Default.GetInstance<NavigationService>().NavigateTo("PivotPage");
                Messenger.Default.Send<NotificationMessage>(new NotificationMessage("Login"));
            }
        }

        private RelayCommand _permissionGrantedCommand = null;
        public RelayCommand PermissionGrantedCommand
        {
            get { return _permissionGrantedCommand ?? (_permissionGrantedCommand = new RelayCommand(ExecutePermissionGrantedCommand)); }
        }

        private async void ExecutePermissionGrantedCommand()
        {
            await StatusBarManager.ShowTextAndProgressAsync("Łączenie konta z aplikacją...");

            var post = new List<KeyValuePair<string, string>>();
            foreach (var n in this.permissionNames)
                post.Add(new KeyValuePair<string, string>("connect[permissions][]", n));

            post.Add(new KeyValuePair<string, string>("__token", this.magicalToken));
            post.Add(new KeyValuePair<string, string>("appnew[submit]", "Połącz z aplikacją"));

            using (var content = new FormUrlEncodedContent(post))
            {
                var response = await client.PostAsync(this.lastURL, content);

                if (!response.IsSuccessStatusCode)
                {
                    Messenger.Default.Send<LoginShowFlyoutMessage>(new LoginShowFlyoutMessage()
                    {
                        FlyoutType = LoginShowFlyoutMessage.FlyoutTypeEnum.Error,
                        ErrorMessage = "Serwer odpowiedział: " + response.StatusCode.ToString(),
                    });

                    return;
                }

                var url = response.RequestMessage.RequestUri.ToString();
                if (url.StartsWith(this.endURL))
                {
                    processFinalURL(url);
                }
            }

            await StatusBarManager.HideProgressAsync();
        }

        #region PasswordVault
        private void SaveCredentials()
        {
            var vault = new PasswordVault();
            var credential = new PasswordCredential(RESOURCE_NAME, Username, Password);
            vault.Add(credential);
        }

        public void RemoveCredentials()
        {
            var vault = new PasswordVault();
            try
            {
                // Removes the credential from the password vault.
                string username = App.ApiService.UserInfo.UserName;
                vault.Remove(vault.Retrieve(RESOURCE_NAME, username));
            }
            catch (Exception)
            {
                // If no credentials have been stored with the given RESOURCE_NAME, an exception
                // is thrown.
            }

        }
        #endregion
    }
}
