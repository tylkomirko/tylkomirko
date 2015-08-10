using MetroLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using WykopSDK.API.Models;
using WykopSDK.Utils;

namespace WykopSDK.API
{
    public class WykopAPI : IDisposable, INotifyPropertyChanged
    {
        private UserInfo _userInfo = null;
        public UserInfo UserInfo
        {
            get { return _userInfo; }

            set 
            { 
                _userInfo = value;
                RaisePropertyChanged();
            }
        }

        public string APPKEY { get; private set; }
        public string SECRETKEY { get; private set; }
        public bool IsWIFIAvailable { get; set; }
        public bool IsNetworkAvailable { get; set; }

        private bool isLoggedIn = false;
        private bool limitExceeded = false;
        private const string baseURL = "http://a.wykop.pl/";

        private RetryHandler _retryHandler = null;
        private RetryHandler retryHandler
        {
            get
            {
                if (_retryHandler == null)
                    _retryHandler = new RetryHandler(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
                return _retryHandler;
            }
        }

        private HttpClient _httpClient = null;
        public HttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                    _httpClient = new HttpClient(retryHandler) { BaseAddress = new Uri(baseURL), Timeout = new TimeSpan(0, 0, 30) };
                return _httpClient;
            }
        }

        private Timer _limitTimer = null;
        private Timer limitTimer
        {
            get
            {
                if(_limitTimer == null)
                    _limitTimer = new Timer(limitTimer_Callback, null, Timeout.Infinite, TimeSpan.FromMinutes(10).Milliseconds);
                return _limitTimer;
            }
        }

        private readonly ILogger _log;

        public delegate void MessageEventHandler(object sender, MessageEventArgs e);
        public event MessageEventHandler MessageReceiver;

        public delegate void NetworkEventHandler(object sender, NetworkEventArgs e);
        public event NetworkEventHandler NetworkStatusChanged;

        public WykopAPI()
        {
            APPKEY = "Q9vny6I5JQ";
            SECRETKEY = "aJaoASCwx9";

            _log = LogManagerFactory.DefaultLogManager.GetLogger<WykopAPI>();
            NetworkInformation.NetworkStatusChanged += (s) => CheckConnection();
            
            LoadUserInfo();

            CheckConnection();
        }

        private void CheckConnection()
        {
            try
            {
                var internetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

                if (internetConnectionProfile == null)
                {
                    IsNetworkAvailable = false;
                    IsWIFIAvailable = false;
                }
                else
                {
                    IsNetworkAvailable = true;
                    /*
                    MainViewModel.ActionOnCurrentCollection(col =>
                    {
                        col._hasMoreItems = true;
                    });
                     * */

                    if (internetConnectionProfile.NetworkAdapter.IanaInterfaceType == 71) // wifi
                        IsWIFIAvailable = true;
                    else
                        IsWIFIAvailable = false;
                }
            }
            catch (Exception e)
            {
                _log.Error("NetworkStatusChanged", e);
            }

            if (NetworkStatusChanged != null)
                NetworkStatusChanged(this, new NetworkEventArgs(IsNetworkAvailable, IsWIFIAvailable));
        }

        #region UserInfo
        private void LoadUserInfo()
        {
            var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            var roamingValues = roamingSettings.Values;
            if (roamingSettings.Containers.ContainsKey("UserInfo"))
            {
                var values = roamingSettings.Containers["UserInfo"].Values;
                UserInfo = new UserInfo();

                UserInfo.AccountKey = (string)values["AccountKey"];
                UserInfo.UserKey = (string)values["UserKey"];
                UserInfo.UserName = (string)values["UserName"];

                UserInfo.IsAppRunning = (bool)values["IsAppRunning"];

                UserInfo.AtNotificationsCount = (int)values["AtNotificationsCount"];
                UserInfo.HashtagNotificationsCount = (int)values["HashtagNotificationsCount"];
                UserInfo.PMNotificationsCount = (int)values["PMNotificationsCount"];

                UserInfo.LastToastDate = DateTime.Parse((string)values["LastToastDate"], null, System.Globalization.DateTimeStyles.RoundtripKind);
            }
        }

        public void SaveUserInfo()
        {
            if (UserInfo != null)
            {
                // FIXME
                //UserInfo.HashtagNotificationsCount = App.NotificationsViewModel.HashtagNotificationsCount;
                //UserInfo.AtNotificationsCount = App.NotificationsViewModel.AtNotificationsCount;
                //UserInfo.PMNotificationsCount = App.NotificationsViewModel.PMNotificationsCount;

                var container = Windows.Storage.ApplicationData.Current.RoamingSettings.CreateContainer("UserInfo", Windows.Storage.ApplicationDataCreateDisposition.Always);
                var values = container.Values;

                values["AccountKey"] = UserInfo.AccountKey;
                values["UserKey"] = UserInfo.UserKey;
                values["UserName"] = UserInfo.UserName;

                values["IsAppRunning"] = UserInfo.IsAppRunning;

                values["AtNotificationsCount"] = UserInfo.AtNotificationsCount;
                values["HashtagNotificationsCount"] = UserInfo.HashtagNotificationsCount;
                values["PMNotificationsCount"] = UserInfo.PMNotificationsCount;

                values["LastToastDate"] = UserInfo.LastToastDate.ToString("o");
            }
        }
        #endregion

        public void Dispose()
        {
            _log.Info("Dispose.");

            HttpClient.Dispose();
            _httpClient = null;

            retryHandler.Dispose();
            _retryHandler = null;

            limitTimer.Dispose();
            _limitTimer = null;
        }

        private void limitTimer_Callback(object state)
        {
            limitExceeded = false;
            limitTimer.Change(Timeout.Infinite, 0); // stop the timer
        }

        #region HelperFunctions
        private async Task<T> deserialize<T>(string URL, SortedDictionary<string, string> post = null, Stream fileStream = null, string fileName = null, CancellationToken ct = default(CancellationToken))
            where T : class
        {
            T result = null;

            var newURL = URL.Replace("appkey,Q9vny6I5JQ", "appkey,XX,");
            var userkeyIndex = newURL.IndexOf("userkey,");
            if (userkeyIndex != -1)
            {
                userkeyIndex += 8;
                var commaIndex = newURL.IndexOf(',', userkeyIndex);
                var userkey = newURL.Substring(userkeyIndex, commaIndex - userkeyIndex);
                if (!string.IsNullOrEmpty(userkey))
                    newURL = newURL.Replace(userkey, "XXX");
            }

            _log.Trace(newURL);

            using (var stream = await getAsync(URL, post, fileStream, fileName))
            {
                if (stream == null)
                    return null;

                using (StreamReader sr = new StreamReader(stream))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    if (stream.Length < 100)
                    {
                        var str = sr.ReadToEnd();
                        if (str.StartsWith(@"{""error"""))
                        {
                            Error err = JsonConvert.DeserializeObject<Error>(str);
                            var errorCode = err.error.code;
                            var message = err.error.message;
                            _log.Warn("API ERROR " + errorCode + ", " + message);

                            if (MessageReceiver != null && (errorCode != 999 && errorCode != 5 && errorCode != 11 && errorCode != 12))
                                MessageReceiver(this, new MessageEventArgs(message, errorCode));

                            if (errorCode == 5) // limit exceeded
                            {
                                this.limitTimer.Change(0, (int)TimeSpan.FromMinutes(10).TotalMilliseconds);
                                limitExceeded = true;
                            }
                            else if (errorCode == 11 || errorCode == 12) // wrong user key
                            {
                                var oldUserKey = UserInfo.UserKey;
                                _log.Warn("old URL: " + URL.Replace("appkey,Q9vny6I5JQ", "appkey,XX,"));
                                await Login(force: true);
                                var updatedURL = URL.Replace(oldUserKey, UserInfo.UserKey);
                                _log.Warn("updated URL: " + updatedURL.Replace("appkey,Q9vny6I5JQ", "appkey,XX,"));
                                return await deserialize<T>(updatedURL, post, fileStream, fileName, ct);
                            }
                            return null;
                        }
                        else
                        {
                            sr.BaseStream.Position = 0;
                        }
                    }

                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Error += serializer_Error;
                    result = serializer.Deserialize<T>(reader);
                }

                return result;
            }
        }

        private void serializer_Error(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            _log.Error("Deserialization error: " + e.ErrorContext.Error);
        }

        private async Task<List<T>> deserializeList<T>(string URL, SortedDictionary<string, string> post = null, CancellationToken ct = default(CancellationToken))
            where T : class
        {
            List<T> list = new List<T>(50);
            JsonSerializer serializer = new JsonSerializer();

            var newURL = URL.Replace("appkey,Q9vny6I5JQ", "appkey,XX,");
            var userkeyIndex = newURL.IndexOf("userkey,");
            if (userkeyIndex != -1)
            {
                userkeyIndex += 8;
                var commaIndex = newURL.IndexOf(',', userkeyIndex);
                var userkey = newURL.Substring(userkeyIndex, commaIndex - userkeyIndex);
                if (!string.IsNullOrEmpty(userkey))
                    newURL = newURL.Replace(userkey, "XXX");
            }

            _log.Trace(newURL);

            using (var stream = await getAsync(URL, post, ct: ct))
            {
                if (stream == null || stream.Length == 0 || stream.Length == 2) // length 2 equals []. essentialy an empty response.
                    return null;

                using (StreamReader sr = new StreamReader(stream))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    if (stream.Length < 100)
                    {
                        var str = sr.ReadToEnd();
                        if (str.StartsWith(@"{""error"""))
                        {
                            Error err = JsonConvert.DeserializeObject<Error>(str);
                            var errorCode = err.error.code;
                            var message = err.error.message;
                            _log.Warn("API ERROR " + errorCode + ", " + message);

                            if (errorCode != 999 && MessageReceiver != null)
                                MessageReceiver(this, new MessageEventArgs(message, errorCode));

                            if (errorCode == 5) // limit exceeded
                            {
                                this.limitTimer.Change(0, (int)TimeSpan.FromMinutes(10).TotalMilliseconds);
                                limitExceeded = true;
                            }
                            else if (errorCode == 11 || errorCode == 12) // wrong user key
                            {
                                var oldUserKey = UserInfo.UserKey;
                                await Login(force: true);
                                var updatedURL = URL.Replace(oldUserKey, UserInfo.UserKey);
                                return await deserializeList<T>(updatedURL, post, ct);
                            }
                            return null;
                        }
                    }
                    
                    JArray array = JArray.Load(reader);
                    foreach (var item in array)
                    {
                        var r = item.CreateReader();
                        var title = item["title"];
                        if(title != null)
                        {
                            // filter out front page entries.
                            r.Skip();
                        }
                        else
                        {
                            var i = serializer.Deserialize<T>(r);
                            list.Add(i);
                        }
                    }
                }

                return list;
            }

        }

        private StreamContent CreateFileContent(Stream stream, string fileName, string contentType)
        {
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"embed\"",
                FileName = "\"" + fileName + "\""
            };
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return fileContent;
        }

        private async Task<Stream> getAsync(string url, SortedDictionary<string, string> post = null, Stream fileStream = null, string fileName = null, CancellationToken ct = default(CancellationToken))
        {
            HttpResponseMessage response;

            using (var content = new MultipartFormDataContent())
            {
                if (post != null)
                {
                    foreach (var pair in post)
                        content.Add(new StringContent(pair.Value), pair.Key);
                }

                if (fileStream != null)
                {
                    fileStream.Position = 0;

                    string mimeType = string.Empty;
                    if (fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg"))
                        mimeType = "image/jpeg";
                    else if (fileName.EndsWith(".png"))
                        mimeType = "image/png";
                    else if (fileName.EndsWith(".gif"))
                        mimeType = "image/gif";

                    content.Add(CreateFileContent(fileStream, fileName, mimeType));
                }

                content.Headers.Add("apisign", this.calculateMD5(url, post));

                try
                {
                    response = await HttpClient.PostAsync(url, content, ct);
                } 
                catch(Exception e)
                {
                    _log.Error("Something when wrong in getAsync: ", e);
                    return null;
                }
            }

            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
            {
                if (MessageReceiver != null)
                    MessageReceiver(this, new MessageEventArgs(response.ReasonPhrase, (int)response.StatusCode));

                return null;
            }

            var stream = await response.Content.ReadAsStreamAsync();
            if (stream == null)
            {
                var message = "Serwer zwrócił pustą odpowiedź.";
                if (MessageReceiver != null)
                    MessageReceiver(this, new MessageEventArgs(message, 0));
            }
            return stream;
        }

        private string calculateMD5(string url, SortedDictionary<string, string> post)
        {
            string str = SECRETKEY + baseURL + url;

            if (post != null)
            {
                foreach (var pair in post)
                    str += pair.Value + ",";
                str = str.Remove(str.Length - 1); // remove last ','
            }

            return Cryptography.EncodeMD5(str);
        }

        #endregion

        #region User Account Management
        public async Task<bool> Login(bool force = false)
        {
            if (!force && limitExceeded)
                return false;

            if (!force && isLoggedIn)
                return true;

            if (UserInfo == null)
                return false;

            if (force)
                _log.Trace("Forced login.");

            string URL = "user/login/appkey," + APPKEY + "/";
            var post = new SortedDictionary<string, string>();
            post.Add("login", UserInfo.UserName);
            post.Add("accountkey", UserInfo.AccountKey);

            var result = await deserialize<User>(URL, post);

            if (result != null)
            {
                UserInfo.UserKey = result.userkey;
                isLoggedIn = true;
                SaveUserInfo();
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Entries getters
        public async Task<IEnumerable<Entry>> GetEntries(int pageIndex, CancellationToken ct = default(CancellationToken), uint firstID = 0)
        {
            if (limitExceeded || pageIndex < 0)
                return null;

            string URL = null;

            if (firstID == 0)
                URL = "stream/index";
            else
                URL = "stream/index/firstid/" + firstID;
                
            if(UserInfo != null)
                URL += "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY + ",page," + pageIndex;
            else
                URL += "/appkey," + APPKEY + ",page," + pageIndex;

            var result = await deserialize<List<Entry>>(URL, ct: ct);
            return result != null ? result.Where(x => !x.Blacklisted) : null;
        }

        public async Task<IEnumerable<Entry>> GetHotEntries(int period, int pageIndex, CancellationToken ct = default(CancellationToken), uint firstID = 0)
        {
            if (limitExceeded || pageIndex < 0)
                return null;

            string URL = null;

            if (firstID == 0)
                URL = "stream/hot";
            else
                URL = "stream/hot/firstid/" + firstID;

            if (UserInfo != null)
                URL += "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY + ",period," + period + ",page," + pageIndex;
            else
                URL += "/appkey," + APPKEY + ",period," + period + ",page," + pageIndex;

            var result = await deserialize<List<Entry>>(URL, ct: ct);
            return result != null ? result.Where(x => !x.Blacklisted) : null;
        }

        public async Task<List<Entry>> GetMyEntries(int pageIndex, CancellationToken ct = default(CancellationToken))
        {
            if (limitExceeded || UserInfo == null || pageIndex < 0)
                return null;

            var URL = "mywykop/index/userkey," + UserInfo.UserKey + ",appkey," + APPKEY + ",page," + pageIndex;

            return await deserializeList<Entry>(URL, ct: ct);
        }

        #endregion

        #region Entry management
        public async Task<Entry> GetEntry(uint id)
        {
            if (limitExceeded || id == 0)
                return null;

            string URL;
            if(UserInfo != null)
                URL = "entries/index/" + id + "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;
            else
                URL = "entries/index/" + id + "/appkey," + APPKEY;

            return await deserialize<Entry>(URL);
        }

        public async Task<uint> AddEntry(NewEntry newEntry, Stream fileStream = null, string fileName = null)
        {
            if (limitExceeded || UserInfo == null || newEntry == null)
                return 0;

            string URL = "entries/add";
            if (newEntry.EntryID != 0)
                URL += "comment/" + newEntry.EntryID;

            URL += "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;

            var post = new SortedDictionary<string, string>();
            post.Add("body", newEntry.Text);
            if (!string.IsNullOrEmpty(newEntry.Embed))
                post.Add("embed", newEntry.Embed);

            var result = await deserialize<EntryIDReply>(URL, post, fileStream, fileName);
            return result != null ? result.ID : 0;
        }

        public async Task<uint> EditEntry(NewEntry entry)
        {
            if (limitExceeded || UserInfo == null || entry == null)
                return 0;

            string URL = "entries/edit";
            if (entry.CommentID != 0)
                URL += "comment/" + entry.EntryID + "/" + entry.CommentID + "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;
            else
                URL += "/" + entry.EntryID + "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;

            var post = new SortedDictionary<string, string>();
            post.Add("body", entry.Text);

            var result = await deserialize<EntryIDReply>(URL, post);
            return result != null ? result.ID : 0;
        }

        public async Task<uint> DeleteEntry(uint id, uint rootID = 0, bool isComment = false)
        {
            if (limitExceeded || UserInfo == null || id == 0)
                return 0;

            string URL = "entries/delete";
            if (isComment)
                URL += "comment/" + rootID;

            URL += "/" + id + "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;

            var result = await deserialize<EntryIDReply>(URL);
            return result != null ? result.ID : 0;
        }

        public async Task<Vote> VoteEntry(uint id, uint commentID = 0, bool upVote = true, bool isItEntry = true)
        {
            if (limitExceeded || UserInfo == null || id == 0)
                return null;

            string URL = "entries/";
            if (upVote)
                URL += "vote/";
            else
                URL += "unvote/";

            if (isItEntry)
                URL += "entry/";
            else
                URL += "comment/";

            URL += id + "/";

            if (!isItEntry)
                URL += commentID + "/";

            URL += "userkey," + UserInfo.UserKey + ",appkey," + APPKEY;

            return await deserialize<Vote>(URL);
        }
        #endregion

        #region Tags
        public async Task<List<Hashtag>> GetPopularTags()
        {
            if (limitExceeded)
                return null;

            string URL = "tags/index/appkey," + APPKEY;

            return await deserialize<List<Hashtag>>(URL);
        }

        public async Task<List<string>> GetUserObservedTags()
        {
            if (limitExceeded || UserInfo == null)
                return null;

            string URL = "user/tags/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;

            return await deserialize<List<string>>(URL);
        }

        public async Task<TaggedEntries> GetTaggedEntries(string hashtag, int pageIndex, CancellationToken ct = default(CancellationToken), uint firstID = 0)
        {
            if (limitExceeded || string.IsNullOrEmpty(hashtag) || pageIndex < 0)
                return null;

            string URL = string.Format("tag/entries/{0}", hashtag.Substring(1));

            if (firstID != 0)
                URL += string.Format("/firstid/{0}", firstID);

            if (UserInfo != null)
                URL += "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY + ",page," + pageIndex;
            else
                URL += "/appkey," + APPKEY + ",page," + pageIndex;

            return await deserialize<TaggedEntries>(URL, ct: ct);
        }

        public async Task<bool> ObserveTag(string tag)
        {
            if (limitExceeded || string.IsNullOrEmpty(tag) || UserInfo == null)
                return false;

            string URL = "tag/observe/" + tag.Substring(1) + "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return (result != null) ? result[0] : false;
        }

        public async Task<bool> UnobserveTag(string tag)
        {
            if (limitExceeded || string.IsNullOrEmpty(tag) || UserInfo == null)
                return false;

            string URL = "tag/unobserve/" + tag.Substring(1) + "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return (result != null) ? result[0] : false;
        }
        #endregion

        #region User profile
        public async Task<Profile> GetProfile(string user)
        {
            if (limitExceeded || string.IsNullOrEmpty(user))
                return null;

            string URL = "profile/index/" + user;
            if(UserInfo != null)
                URL += "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;
            else
                URL += "/appkey," + APPKEY;

            return await deserialize<Profile>(URL);
        }

        public async Task<bool> ObserveUser(string user)
        {
            if (limitExceeded || string.IsNullOrEmpty(user) || UserInfo == null)
                return false;

            string URL = "profile/observe/" + user + "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return (result != null) ? result[0] : false;
        }

        public async Task<bool> UnobserveUser(string user)
        {
            if (limitExceeded || string.IsNullOrEmpty(user) || UserInfo == null)
                return false;

            string URL = "profile/unobserve/" + user + "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return (result != null) ? result[0] : false;
        }

        public async Task<List<Entry>> GetUserEntries(string user, int pageIndex)
        {
            if (limitExceeded || string.IsNullOrEmpty(user) || pageIndex < 0)
                return null;

            string URL = "profile/entries/" + user;
            if (UserInfo != null)
                URL += "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY + ",page," + pageIndex;
            else
                URL += "/appkey," + APPKEY + ",page," + pageIndex;

            return await deserialize<List<Entry>>(URL);
        }
        #endregion

        #region Notifications
        public async Task<List<Notification>> GetNotifications(uint page)
        {
            if (limitExceeded || UserInfo == null)
                return null;

            string URL = "mywykop/notifications/userkey," + UserInfo.UserKey + ",appkey," + APPKEY + ",page," + page;

            return await deserialize<List<Notification>>(URL);
        }

        public async Task<NotificationsCount> GetNotificationsCount()
        {
            if (limitExceeded || UserInfo == null)
                return null;

            string URL = "mywykop/notificationscount/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;

            return await deserialize<NotificationsCount>(URL);
        }

        public async Task<List<Notification>> GetHashtagNotifications(uint page)
        {
            if (limitExceeded || UserInfo == null)
                return null;

            string URL = "mywykop/hashtagsnotifications/userkey," + UserInfo.UserKey + ",appkey," + APPKEY + ",page," + page;

            return await deserialize<List<Notification>>(URL);
        }

        public async Task<NotificationsCount> GetHashtagNotificationsCount()
        {
            if (limitExceeded || UserInfo == null)
                return null;

            string URL = "mywykop/hashtagsnotificationscount/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;

            return await deserialize<NotificationsCount>(URL);
        }

        public async Task<bool> ReadNotifications()
        {
            if (limitExceeded || UserInfo == null)
                return false;

            string URL = "mywykop/ReadNotifications/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;

            await deserialize<List<object>>(URL);
            return true;
        }

        public async Task<bool> ReadHashtagNotifications()
        {
            if (limitExceeded || UserInfo == null)
                return false;

            string URL = "mywykop/ReadHashTagsNotifications/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;

            await deserialize<List<object>>(URL);
            return true;
        }

        public async Task<bool> MarkAsReadNotification(uint id)
        {
            if (limitExceeded || UserInfo == null || id == 0)
                return false;

            string URL = "mywykop/markasreadnotification/" + id + "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;

            using (var stream = await getAsync(URL))
            using (var sr = new StreamReader(stream))
            {
                var str = sr.ReadToEnd();
                if (str == "true")
                    return true;
                else
                    return false;
            }
        }

        #endregion

        #region PM
        public async Task<List<Conversation>> GetConversations(bool force = false)
        {
            if (!force)
            {
                List<Conversation> tmp = null;
                tmp = await WykopSDK.LocalStorage.ReadConversations();
                if (tmp != null)
                    return tmp;
            }

            if (limitExceeded || UserInfo == null)
                return null;

            string URL = "pm/ConversationsList/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;
            var reply = await deserialize<List<Conversation>>(URL);

            if (reply != null)
                return reply.Where(x => x.AuthorGroup != UserGroup.Deleted).ToList();
            else
                return null;
        }

        public async Task<List<PM>> GetPMs(string username)
        {
            if (limitExceeded || UserInfo == null || string.IsNullOrEmpty(username))
                return null;

            string URL = "pm/Conversation/" + username + "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;

            return await deserialize<List<PM>>(URL);
        }

        public async Task<bool> SendPM(NewEntry newEntry, string username, Stream fileStream = null, string fileName = null)
        {
            if (limitExceeded || UserInfo == null || newEntry == null || string.IsNullOrEmpty(username))
                return false;

            string URL = "pm/SendMessage/" + username + "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;

            var post = new SortedDictionary<string, string>();
            post.Add("body", newEntry.Text);
            if (!string.IsNullOrEmpty(newEntry.Embed))
                post.Add("embed", newEntry.Embed);

            var result = await deserialize<List<bool>>(URL, post, fileStream, fileName);
            return result != null ? result[0] : false;
        }

        public async Task<bool> DeleteConversation(string username)
        {
            if (limitExceeded || UserInfo == null || string.IsNullOrEmpty(username))
                return false;

            string URL = "pm/DeleteConversation/" + username + "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return result != null ? result[0] : false;
        }
        #endregion

        #region Blacklist
        public async Task<bool> BlockTag(string tag)
        {
            if (limitExceeded || UserInfo == null || string.IsNullOrEmpty(tag))
                return false;

            string URL = "tag/block/" + tag.Substring(1) + "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return (result != null) ? result[0] : false;
        }

        public async Task<bool> UnblockTag(string tag)
        {
            if (limitExceeded || UserInfo == null || string.IsNullOrEmpty(tag))
                return false;

            string URL = "tag/unblock/" + tag.Substring(1) + "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return (result != null) ? result[0] : false;
        }

        public async Task<bool> BlockUser(string username)
        {
            if (limitExceeded || UserInfo == null || string.IsNullOrEmpty(username))
                return false;

            string URL = "profile/block/" + username + "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return result[0];
        }

        public async Task<bool> UnblockUser(string username)
        {
            if (limitExceeded || UserInfo == null || string.IsNullOrEmpty(username))
                return false;

            string URL = "profile/unblock/" + username + "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return result[0];
        }
        #endregion

        #region Favourites
        public async Task<List<Entry>> GetFavourites(CancellationToken ct = default(CancellationToken))
        {
            if (limitExceeded || UserInfo == null)
                return null;

            string URL = "favorites/entries/userkey," + UserInfo.UserKey + ",appkey," + APPKEY + ",page,0";

            return await deserialize<List<Entry>>(URL, ct: ct);
        }

        public async Task<UserFavorite> AddToFavourites(uint id)
        {
            if (limitExceeded || UserInfo == null || id == 0)
                return null;

            string URL = "entries/favorite/" + id + "/userkey," + UserInfo.UserKey + ",appkey," + APPKEY;

            return await deserialize<UserFavorite>(URL);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
        }

    }
}
