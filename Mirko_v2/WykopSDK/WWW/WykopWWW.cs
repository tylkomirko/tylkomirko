using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WykopSDK.API;
using WykopSDK.Utils;

namespace WykopSDK.WWW
{
    public class WykopWWW : IDisposable
    {
        private const string baseURL = "https://wykop.pl/";

        private RetryHandler _retryHandler = null;
        private RetryHandler retryHandler
        {
            get
            {
                if (_retryHandler == null)
                    _retryHandler = new RetryHandler(new HttpClientHandler() { AllowAutoRedirect = true, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
                return _retryHandler;
            }
        }

        private HttpClient _httpClient = null;
        public HttpClient httpClient
        {
            get
            {
                if (_httpClient == null)
                    _httpClient = new HttpClient(retryHandler) { BaseAddress = new Uri(baseURL), Timeout = new TimeSpan(0, 0, 30) };
                return _httpClient;
            }
        }

        private HtmlParser _parser = null;
        public HtmlParser parser
        {
            get { return _parser ?? (_parser = new HtmlParser()); }
        }

        public async Task ConnectAccount(
            string username, string password,
            string appKey, string secretKey,
            Action<string> serverResponseCallback = null,
            Action<string> errorCallback = null,
            Action<UserInfo> successCallback = null)
        {
            const string endURL = "http://www.wykop.pl/user/ConnectSuccess/";

            string token = null;
            string lastURL = null;
            List<string> permissionNames = null;

            string startURL = "https://a.wykop.pl/user/connect/appkey," + appKey;
            string redirectURL = "https://www.wykop.pl/user/ConnectSuccess";

            var encodedRedirectURL = WebUtility.UrlEncode(Cryptography.EncodeBase64(redirectURL));
            startURL += ",redirect," + encodedRedirectURL + ",secure," + Cryptography.EncodeMD5(secretKey + redirectURL);

            var post = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("login[login]", username),
                new KeyValuePair<string, string>("login[password]", password),
            };

            using (var content = new FormUrlEncodedContent(post))
            using (var response = await httpClient.PostAsync(startURL, content))
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                if (!response.IsSuccessStatusCode)
                {
                    if (serverResponseCallback != null)
                        serverResponseCallback(response.StatusCode.ToString());
                    return;
                }

                lastURL = response.RequestMessage.RequestUri.ToString();
                if (lastURL.StartsWith(endURL))
                {
                    // account is already connected to app. save credentials and quit.
                    ConnectAccount_ProcessFinalURL(lastURL, successCallback);
                    WykopSDK.VaultStorage.SaveCredentials(username, password);

                    return;
                }

                var doc = parser.Parse(stream);
                if (doc == null) return;

                var errorDiv = doc.QuerySelector(".error");
                if (errorDiv != null)
                {
                    // website returned an error. all we can do is report it and quit.
                    if (errorCallback != null)
                        errorCallback(errorDiv.InnerHtml.Trim());
                    return;
                }

                // extract token and permission names and then submit POST request
                var labels = doc.QuerySelector("form").QuerySelectorAll("label").Cast<IHtmlLabelElement>();
                permissionNames = labels.Select(label =>
                {
                    var input = label.Children[0] as IHtmlInputElement;
                    return input.Value;
                }).ToList();

                var tokenElement = doc.QuerySelector("input#__token") as IHtmlInputElement;
                token = tokenElement.Value;
            }

            post = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("__token", token),
                new KeyValuePair<string, string>("appnew[submit]", "Połącz z aplikacją"),
            };

            foreach (var name in permissionNames)
                post.Add(new KeyValuePair<string, string>("connect[permissions][]", name));

            using (var content = new FormUrlEncodedContent(post))
            using (var response = await httpClient.PostAsync(lastURL, content))
            {
                if (!response.IsSuccessStatusCode)
                {
                    if (serverResponseCallback != null)
                        serverResponseCallback(response.StatusCode.ToString());
                    return;
                }

                var url = response.RequestMessage.RequestUri.ToString();
                if (url.StartsWith(endURL))
                {
                    ConnectAccount_ProcessFinalURL(url, successCallback);
                    WykopSDK.VaultStorage.SaveCredentials(username, password);
                }
            }
        }

        private void ConnectAccount_ProcessFinalURL(string url, Action<UserInfo> successCallback)
        {
            var index = url.IndexOf("connectData=") + 12;
            var base64Encoded = url.Substring(index);
            var json = Cryptography.DecodeBase64(base64Encoded);
            var result = JsonConvert.DeserializeObject<ConnectAccountReply>(json);

            if (successCallback != null)
            {
                var userInfo = new UserInfo()
                {
                    UserName = result.Username,
                    AccountKey = result.AccountKey,
                    UserKey = "maciejkochamcie",
                };

                successCallback(userInfo);
            }
        }

        public async Task<bool> Login(string username, string password)
        {
            string token = null;
            string loginURL = null;

            using (var response = await httpClient.GetAsync(baseURL + "zaloguj/"))
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var doc = parser.Parse(stream);
                if (doc == null) return false;

                var form = doc.QuerySelector("form.bspace-big.login-form") as IHtmlFormElement;
                if (form == null) return false;
                loginURL = form.Action;

                var tokenElement = doc.QuerySelector("input#__token") as IHtmlInputElement;
                if (tokenElement == null) return false;

                token = tokenElement.Value;
            }

            var post = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("user[username]", username),
                new KeyValuePair<string, string>("user[password]", password),
                new KeyValuePair<string, string>("__token", token),
            };

            using (var content = new FormUrlEncodedContent(post))
            using (var response = await httpClient.PostAsync(loginURL, content))
            {
                var finalUrl = response.RequestMessage.RequestUri.AbsoluteUri;
                if (finalUrl.Equals("http://www.wykop.pl/") || finalUrl.Equals("https://www.wykop.pl/"))
                    return true;
                else
                    return false;
            }
        }

        public async Task<List<string>> GetObservedUsers()
        {
            var users = new List<string>();

            using (var response = await httpClient.GetAsync(baseURL + "moj/notatki-o-uzytkownikach/"))
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var doc = parser.Parse(stream);
                if (doc == null) return null;

                var divs = doc.QuerySelector("#observedUsers").QuerySelectorAll("div");
                foreach (var item in divs)
                {
                    var ahref = item.Children[0] as IHtmlAnchorElement;
                    var url = ahref.Href;

                    var splitUrl = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    users.Add(splitUrl.Last());
                }
            }

            return users;
        }

        public async Task<List<string>> GetBlacklistedUsers()
        {
            var users = new List<string>();

            using (var response = await httpClient.GetAsync(baseURL + "ustawienia/czarne-listy/"))
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var doc = parser.Parse(stream);
                if (doc == null) return null;

                var divs = doc.QuerySelector(@"div[data-type=""users""]").QuerySelectorAll("div");
                foreach (var item in divs)
                {
                    var ahref = item.Children[1] as IHtmlAnchorElement;
                    var url = ahref.Href;

                    var splitUrl = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    users.Add(splitUrl.Last());
                }
            }

            return users;
        }

        public async Task GetUserNotes()
        {
            var notes = new Dictionary<string, string>();

            using (var response = await httpClient.GetAsync(baseURL + "moj/notatki-o-uzytkownikach/"))
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var doc = parser.Parse(stream);
                if (doc == null) return;

                var paragraphs = doc.QuerySelector("ul#notesList").QuerySelectorAll("p");
                foreach (var item in paragraphs)
                {
                    var temp = item.TextContent.Trim();
                    var split = temp.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var username = split[0];
                    var note = split.Last().Trim();

                    notes.Add(username, note);
                }
            }
        }

        public async Task SetUsernote(string username, string note)
        {
            /*
            string hash = null;

            using (var response = await httpClient.GetAsync(baseURL + "ludzie/" + username))
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var doc = parser.Parse(stream);
                if (doc == null) return;

                var script = doc.QuerySelector("script") as IHtmlScriptElement;
                var lines = script.InnerHtml.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var hashLine = lines.First(x => x.Contains("hash")).Trim();

                var split = hashLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                hash = split.Last().Trim(new char[] { '"', ',' });
            }*/


            var post = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("profile[note]", note),
            };

            //string url = baseURL + "ajax2/ludzie/updatenote/" + username + "//hash/" + hash;           
            string url = "http://www.wykop.pl/ajax2/ludzie/updatenote/LubieLiscie//hash/a772444b096c97423871a5832f3dbbd6-1438893069";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            using (var content = new FormUrlEncodedContent(post))
            {
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                request.Headers.Add("Referer", "http://www.wykop.pl/ludzie/LubieLiscie/");
                request.Content = content;
                using (var response = await httpClient.SendAsync(request))
                {
                    var str = await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<List<string>> GetBlacklistedTags()
        {
            var tags = new List<string>();

            using (var response = await httpClient.GetAsync(baseURL + "ustawienia/czarne-listy/"))
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var doc = parser.Parse(stream);
                if (doc == null) return null;

                var spans = doc.QuerySelector(@"div[data-type=""hashtags""]").QuerySelectorAll("span");
                foreach (var item in spans)
                {
                    var ahref = item.Children[0] as IHtmlAnchorElement;
                    var url = ahref.Href;

                    var splitUrl = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    tags.Add(splitUrl.Last());
                }
            }

            return tags;
        }

        public async Task ForgotPassword(string email)
        {
            string token = null;
            string url = baseURL + "zaloguj/odzyskaj-haslo/";

            using (var response = await httpClient.GetAsync(url))
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var doc = parser.Parse(stream);
                if (doc == null) return;

                var tokenElement = doc.QuerySelector("input#__token") as IHtmlInputElement;
                if (tokenElement == null) return;

                token = tokenElement.Value;
            }

            var post = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("user[mail]", email),
                new KeyValuePair<string, string>("__token", token),
            };

            using (var content = new FormUrlEncodedContent(post))
            using (var response = await httpClient.PostAsync(url, content))
            {
                var finalUrl = response.RequestMessage.RequestUri.AbsoluteUri;
            }
        }

        public void Dispose()
        {
            if (httpClient != null)
                httpClient.Dispose();
        }
    }
}
