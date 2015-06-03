using MetroLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;
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
using WykopAPI.JSON;
using WykopAPI.Models;

namespace WykopAPI
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
        public HttpClient httpClient
        {
            get
            {
                if (_httpClient == null)
                    _httpClient = new HttpClient(retryHandler) { BaseAddress = new Uri(baseURL) };
                return _httpClient;
            }
        }

        private LocalStorage _localStorage = null;
        public LocalStorage LocalStorage
        {
            get { return _localStorage ?? (_localStorage = new LocalStorage()); }
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

        public int CallCount
        {
            get
            {
                var settings = Windows.Storage.ApplicationData.Current.LocalSettings.Values;
                if (settings.ContainsKey("CallCount"))
                    return (int)settings["CallCount"];
                else
                    return 0;
            }

            set
            {
                var settings = Windows.Storage.ApplicationData.Current.LocalSettings.Values;
                if(settings.ContainsKey("CallCountUpdateTime"))
                {
                    var time = DateTime.FromBinary((long)settings["CallCountUpdateTime"]);
                    if (DateTime.Now - time > new TimeSpan(1, 0, 0))
                        settings["CallCount"] = 0;
                    else
                        settings["CallCount"] = value;
                }
                else
                {
                    settings["CallCount"] = value;
                }

                settings["CallCountUpdateTime"] = DateTime.Now.ToBinary();
                RaisePropertyChanged();
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
                UserInfo.IsPushEnabled = (bool)values["IsPushEnabled"];

                UserInfo.AtNotificationsCount = (int)values["AtNotificationsCount"];
                UserInfo.HashtagNotificationsCount = (int)values["HashtagNotificationsCount"];
                UserInfo.PMNotificationsCount = (int)values["PMNotificationsCount"];

                UserInfo.LastToastDate = DateTime.Parse((string)values["LastToastDate"], null, System.Globalization.DateTimeStyles.RoundtripKind);
            }
        }

        private void SaveUserInfo()
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
                values["IsPushEnabled"] = UserInfo.IsPushEnabled;

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

            httpClient.Dispose();
            _httpClient = null;

            retryHandler.Dispose();
            _retryHandler = null;

            limitTimer.Dispose();
            _limitTimer = null;
        }

        private void limitTimer_Callback(object state)
        {
            this.limitExceeded = false;
            this.limitTimer.Change(Timeout.Infinite, 0); // stop the timer
        }

        #region HelperFunctions

        private async Task<T> deserialize<T>(string URL, SortedDictionary<string, string> post = null, Stream fileStream = null, string fileName = null)
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

                CallCount++;

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
                                MessageReceiver(this, new MessageEventArgs("Kod " + errorCode + ": " + message));

                            if (errorCode == 5) // limit exceeded
                            {
                                this.limitTimer.Change(0, (int)TimeSpan.FromMinutes(10).TotalMilliseconds);
                                this.limitExceeded = true;
                            }
                            else if (errorCode == 11 || errorCode == 12) // wrong user key
                            {
                                var oldUserKey = UserInfo.UserKey;
                                _log.Warn("old URL: " + URL.Replace("appkey,Q9vny6I5JQ", "appkey,XX,"));
                                await login(force: true);
                                var updatedURL = URL.Replace(oldUserKey, UserInfo.UserKey);
                                _log.Warn("updated URL: " + updatedURL.Replace("appkey,Q9vny6I5JQ", "appkey,XX,"));
                                return await deserialize<T>(updatedURL, post, fileStream, fileName);
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

        private async Task<List<T>> deserializeList<T>(string URL, SortedDictionary<string, string> post = null)
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

            using (var stream = await getAsync(URL, post))
            {
                if (stream == null || stream.Length == 0 || stream.Length == 2) // length 2 equals []. essentialy an empty response.
                    return null;

                CallCount++;

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
                                MessageReceiver(this, new MessageEventArgs("Kod " + errorCode + ": " + message));

                            if (errorCode == 5) // limit exceeded
                            {
                                this.limitTimer.Change(0, (int)TimeSpan.FromMinutes(10).TotalMilliseconds);
                                this.limitExceeded = true;
                            }
                            else if (errorCode == 11 || errorCode == 12) // wrong user key
                            {
                                var oldUserKey = UserInfo.UserKey;
                                await login(force: true);
                                var updatedURL = URL.Replace(oldUserKey, UserInfo.UserKey);
                                return await deserializeList<T>(updatedURL, post);
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

        private async Task<Stream> getAsync(string url, SortedDictionary<string, string> post = null, Stream fileStream = null, string fileName = null)
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

                response = await httpClient.PostAsync(url, content);
            }

            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
            {
                var message = response.StatusCode + ": " + response.ReasonPhrase;
                if (MessageReceiver != null)
                    MessageReceiver(this, new MessageEventArgs(message));

                return null;
            }

            var stream = await response.Content.ReadAsStreamAsync();
            if (stream == null)
            {
                var message = "Serwer zwrócił pustą odpowiedź.";
                if (MessageReceiver != null)
                    MessageReceiver(this, new MessageEventArgs(message));
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

            return MD5.Encode(str);
        }

        #endregion

        #region User Account Management
        public async Task<bool> login(bool force = false)
        {
            if (!force && this.limitExceeded)
                return false;

            if (!force && this.isLoggedIn)
                return true;

            if (UserInfo == null)
                return false;

            if (force)
                _log.Trace("Forced login.");

            string URL = "user/login/appkey," + this.APPKEY + "/";
            var post = new SortedDictionary<string, string>();
            post.Add("login", UserInfo.UserName);
            post.Add("accountkey", UserInfo.AccountKey);

            var result = await deserialize<User>(URL, post);

            if (result != null)
            {
                UserInfo.UserKey = result.userkey;
                this.isLoggedIn = true;
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

        public async Task<IEnumerable<Entry>> getEntries(int pageIndex)
        {
            if (this.limitExceeded)
                return null;

            if (UserInfo != null)
            {
                var URL = "stream/index/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY + ",page," + pageIndex;

                var result = await deserialize<List<Entry>>(URL);
                if (result != null)
                    return result.Where(x => !x.Blacklisted);
                else
                    return null;
            }
            else
            {
                var URL = "stream/index/appkey," + this.APPKEY + ",page," + pageIndex;

                var result = await deserialize<List<Entry>>(URL);
                return result;
            }
        }

        public async Task<IEnumerable<Entry>> getEntries(uint id, int pageIndex)
        {
            if (this.limitExceeded)
                return null;

            if (UserInfo != null)
            {
                var URL = "stream/index/firstid/" + id + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY + ",page," + pageIndex;

                var result = await deserialize<List<Entry>>(URL);
                if (result != null)
                    return result.Where(x => !x.Blacklisted);
                else
                    return null;
            }
            else
            {
                var URL = "stream/index/firstid/" + id + "/appkey," + this.APPKEY + ",page," + pageIndex;

                var result = await deserialize<List<Entry>>(URL);
                return result;
            }
        }

        public async Task<IEnumerable<Entry>> getHotEntries(int period, int pageIndex)
        {
            if (this.limitExceeded)
                return null;

            if (UserInfo != null)
            {
                var URL = "stream/hot/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY + ",period," + period + ",page," + pageIndex;

                var result = await deserialize<List<Entry>>(URL);
                if (result != null)
                    return result.Where(x => !x.Blacklisted);
                else
                    return null;
            }
            else
            {
                var URL = "stream/hot/appkey," + this.APPKEY + ",period," + period + ",page," + pageIndex;

                var result = await deserialize<List<Entry>>(URL);
                return result;
            }
        }

        public async Task<IEnumerable<Entry>> getHotEntries(int period, uint id, uint pageIndex)
        {
            if (this.limitExceeded)
                return null;

            var URL = "stream/hot/firstid/" + id + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY + ",period," + period + ",page," + pageIndex;

            var result = await deserialize<List<Entry>>(URL);
            if (result != null)
                return result.Where(x => !x.Blacklisted);
            else
                return null;
        }

        public async Task<List<Entry>> getMyEntries(int pageIndex)
        {
            if (this.limitExceeded || UserInfo == null)
                return null;

            var URL = "mywykop/index/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY + ",page," + pageIndex;

            return await deserializeList<Entry>(URL);
        }

        public async Task<List<Entry>> getMyEntries(uint firstID, int pageIndex)
        {
            if (this.limitExceeded || UserInfo == null)
                return null;

            var URL = "mywykop/index/firstid/" + firstID + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY + ",page," + pageIndex;

            return await deserializeList<Entry>(URL);
        }

        #endregion

        #region Entry management

        public async Task<Entry> getEntry(uint id)
        {
            if (this.limitExceeded)
                return null;

            string URL = "entries/index/" + id + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;

            return await deserialize<Entry>(URL);
        }

        public async Task<string> addEntry(NewEntry newEntry)
        {
            if (this.limitExceeded || UserInfo == null)
                return null;

            string URL = "entries/add";
            if (newEntry.IsReply)
                URL += "comment/" + newEntry.ID;

            URL += "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;

            var post = new SortedDictionary<string, string>();
            post.Add("body", newEntry.Text);
            if (newEntry.Embed != null)
                post.Add("embed", newEntry.Embed);

            var stream = newEntry.FileStream;
            var fileName = newEntry.FileName;

            var result = await deserialize<EntryIDReply>(URL, post, stream, fileName);
            if (result != null)
                return result.id;
            else
                return null;
        }

        public async Task<string> editEntry(NewEntry entry)
        {
            if (this.limitExceeded)
                return null;

            string URL = "entries/edit";
            if (entry.IsReply)
                URL += "comment/" + entry.ID + "/" + entry.CommentID + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;
            else
                URL += "/" + entry.ID + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;

            var post = new SortedDictionary<string, string>();
            post.Add("body", entry.Text);

            var result = await deserialize<EntryIDReply>(URL, post);
            if (result != null)
                return result.id;
            else
                return null;
        }

        public async Task<string> deleteEntry(uint id, uint rootID = 0, bool isComment = false)
        {
            if (this.limitExceeded)
                return null;

            string URL = "entries/delete";
            if (isComment)
                URL += "comment/" + rootID;

            URL += "/" + id + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;

            var result = await deserialize<EntryIDReply>(URL);
            if (result != null)
                return result.id;
            else
                return null;
        }

        public async Task<Vote> voteEntry(uint id, uint commentID = 0, bool upVote = true, bool isItEntry = true)
        {
            if (this.limitExceeded)
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

            URL += "userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;

            return await deserialize<Vote>(URL);
        }

        #endregion

        #region Tags

        public async Task<List<Hashtag>> getPopularTags()
        {
            if (this.limitExceeded || UserInfo == null)
                return null;

            string URL = "tags/index/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;

            return await deserialize<List<Hashtag>>(URL);
        }

        public async Task<List<string>> getUserObservedTags()
        {
            if (this.limitExceeded || UserInfo == null)
                return null;

            string URL = "user/tags/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;

            return await deserialize<List<string>>(URL);
        }

        public async Task<TaggedEntries> getTaggedEntries(string hashtag, int pageIndex)
        {
            if (this.limitExceeded)
                return null;

            if (UserInfo != null)
            {
                var URL = "tag/entries/" + hashtag.Substring(1) + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY + ",page," + pageIndex;

                var result = await deserialize<TaggedEntries>(URL);
                return result;
            }
            else
            {
                string URL = "tag/entries/" + hashtag.Substring(1) + "/appkey," + this.APPKEY + ",page," + pageIndex;

                var result = await deserialize<TaggedEntries>(URL);
                return result;
            }
        }

        public async Task<TaggedEntries> getTaggedEntries(string hashtag, int firstID, uint pageIndex)
        {
            if (this.limitExceeded)
                return null;

            string URL = "tag/entries/" + hashtag + "/firstid/" + firstID + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY + ",page," + pageIndex;

            return await deserialize<TaggedEntries>(URL);
        }

        public async Task<bool> observeTag(string tag)
        {
            if (this.limitExceeded || string.IsNullOrEmpty(tag))
                return false;

            string URL = "tag/observe/" + tag.Substring(1) + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return (result != null) ? result[0] : false;
        }

        public async Task<bool> unobserveTag(string tag)
        {
            if (this.limitExceeded || string.IsNullOrEmpty(tag))
                return false;

            string URL = "tag/unobserve/" + tag.Substring(1) + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return (result != null) ? result[0] : false;
        }

        #endregion

        #region User profile
        public async Task<Profile> getProfile(string username)
        {
            if (this.limitExceeded)
                return null;

            string URL = "profile/index/" + username + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;
            return await deserialize<Profile>(URL);
        }

        public async Task<bool> observeUser(string user)
        {
            if (this.limitExceeded)
                return false;

            string URL = "profile/observe/" + user + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return result[0];
        }

        public async Task<bool> unobserveUser(string user)
        {
            if (this.limitExceeded)
                return false;

            string URL = "profile/unobserve/" + user + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return result[0];
        }
        #endregion

        #region Notifications

        public async Task<List<Notification>> getNotifications(uint page)
        {
            if (this.limitExceeded || UserInfo == null)
                return null;

            string URL = "mywykop/notifications/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY + ",page," + page;

            return await deserialize<List<Notification>>(URL);
        }

        public async Task<NotificationsCount> getNotificationsCount()
        {
            if (this.limitExceeded || UserInfo == null)
                return null;

            string URL = "mywykop/notificationscount/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;

            return await deserialize<NotificationsCount>(URL);
        }

        public async Task<List<Notification>> getHashtagNotifications(uint page)
        {
            if (this.limitExceeded || UserInfo == null)
                return null;

            string URL = "mywykop/hashtagsnotifications/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY + ",page," + page;

            return await deserialize<List<Notification>>(URL);
        }

        public async Task<NotificationsCount> getHashtagNotificationsCount()
        {
            if (this.limitExceeded || UserInfo == null)
                return null;

            string URL = "mywykop/hashtagsnotificationscount/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;

            return await deserialize<NotificationsCount>(URL);
        }

        public async Task<bool> readNotifications()
        {
            if (this.limitExceeded || UserInfo == null)
                return false;

            string URL = "mywykop/ReadNotifications/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;

            await deserialize<List<object>>(URL);
            return true;
        }

        public async Task<bool> readHashtagNotifications()
        {
            if (this.limitExceeded || UserInfo == null)
                return false;

            string URL = "mywykop/ReadHashTagsNotifications/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;

            await deserialize<List<object>>(URL);
            return true;
        }

        public async Task<bool> markAsReadNotification(uint id)
        {
            if (this.limitExceeded || UserInfo == null)
                return false;

            string URL = "mywykop/markasreadnotification/" + id + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;

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

        public async Task<List<Conversation>> getConversations(bool force = false)
        {
            if (!force)
            {
                List<Conversation> tmp = null;
                tmp = await LocalStorage.ReadConversations();
                if (tmp != null)
                    return tmp;
            }

            if (this.limitExceeded || UserInfo == null)
                return null;

            string URL = "pm/ConversationsList/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;
            var reply = await deserialize<List<Conversation>>(URL);

            if (reply != null)
                return reply.Where(x => x.AuthorGroup != UserGroup.Deleted).ToList();
            else
                return null;
        }

        public async Task<List<PM>> getPMs(string userName)
        {
            if (this.limitExceeded)
                return null;

            string URL = "pm/Conversation/" + userName + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;

            return await deserialize<List<PM>>(URL);
        }

        public async Task<bool> sendPM(NewEntry newEntry, string userName)
        {
            if (this.limitExceeded)
                return false;

            string URL = "pm/SendMessage/" + userName + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;

            var post = new SortedDictionary<string, string>();
            post.Add("body", newEntry.Text);
            if (!string.IsNullOrEmpty(newEntry.Embed))
                post.Add("embed", newEntry.Embed);

            var result = await deserialize<List<bool>>(URL, post, newEntry.FileStream, newEntry.FileName);
            return result != null ? result[0] : false;
        }

        public async Task<bool> deleteConversation(string userName)
        {
            if (this.limitExceeded)
                return false;

            string URL = "pm/DeleteConversation/" + userName + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return result != null ? result[0] : false;
        }
        #endregion

        #region Blacklist
        public async Task<bool> blockTag(string tag)
        {
            if (this.limitExceeded || string.IsNullOrEmpty(tag))
                return false;

            string URL = "tag/block/" + tag.Substring(1) + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return (result != null) ? result[0] : false;
        }

        public async Task<bool> unblockTag(string tag)
        {
            if (this.limitExceeded || string.IsNullOrEmpty(tag))
                return false;

            string URL = "tag/unblock/" + tag.Substring(1) + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return (result != null) ? result[0] : false;
        }

        public async Task<bool> blockUser(string name)
        {
            if (this.limitExceeded)
                return false;

            string URL = "profile/block/" + name + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return result[0];
        }

        public async Task<bool> unblockUser(string name)
        {
            if (this.limitExceeded)
                return false;

            string URL = "profile/unblock/" + name + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;
            var result = await deserialize<List<bool>>(URL);
            return result[0];
        }
        #endregion

        #region Favourites

        public async Task<List<Entry>> getFavourites()
        {
            if (this.limitExceeded || UserInfo == null)
                return null;

            string URL = "favorites/entries/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY + ",page,0";

            return await deserialize<List<Entry>>(URL);
        }

        public async Task<UserFavorite> addToFavourites(uint id)
        {
            if (this.limitExceeded)
                return null;

            string URL = "entries/favorite/" + id + "/userkey," + UserInfo.UserKey + ",appkey," + this.APPKEY;

            return await deserialize<UserFavorite>(URL);
        }

        #endregion

        #region Functions that don't use API
        public async Task<bool> isUserOnline(string username)
        {
            using (var client = new HttpClient()) // whelp
            using (var reply = await client.GetAsync("http://wykop.pl/ludzie/" + username))
            {
                if(reply.IsSuccessStatusCode)
                {
                    using (var stream = await reply.Content.ReadAsStreamAsync())
                    using (var streamReader = new StreamReader(stream))
                    {
                        while(!streamReader.EndOfStream)
                        {
                            var line = await streamReader.ReadLineAsync();
                            if (line == "<span style=\"color: green; line-height: 18px; font-size: 40px; font-weight: bold; vertical-align: middle;\">\u2022</span>")
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
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
