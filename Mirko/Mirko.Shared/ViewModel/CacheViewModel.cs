using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using MetroLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using WykopSDK.Utils;

namespace Mirko.ViewModel
{
    public class CacheViewModel : ViewModelBase
    {
        private readonly TimeSpan FileLifeSpan = new TimeSpan(12, 0, 0);
        private readonly ILogger Logger = null;
        private readonly StorageFolder TempFolder = null;

        public Action GetPopularHashtags = null;
        public Action GetObservedUsers = null;

        public CacheViewModel()
        {
            TempFolder = ApplicationData.Current.TemporaryFolder;
            Logger = LogManagerFactory.DefaultLogManager.GetLogger<CacheViewModel>();

            GetPopularHashtags = new Action(async () => await DownloadPopularHashtags().ConfigureAwait(false));
            GetObservedUsers = new Action(async () =>
            {
                var users = await Task.Run<List<string>>(async () => 
                {
                    return await App.WWWService.GetObservedUsers().ConfigureAwait(false);
                });

                if (users != null)
                {
                    await DispatcherHelper.RunAsync(() =>
                    {
                        ObservedUsers.Clear();
                        ObservedUsers.AddRange(users.Select(x => "@" + x));
                    });
                }
            });

            Messenger.Default.Register<NotificationMessage>(this, ReadMessage);
            Messenger.Default.Register<NotificationMessage<string>>(this, ReadMessage);
        }

        private async void ReadMessage(NotificationMessage obj)
        {
            if (obj.Notification == "Logout")
                await Logout().ConfigureAwait(false);
            else if (obj.Notification == "Update ObservedHashtags")
                await DownloadObservedHashtags().ConfigureAwait(false);
            else if (obj.Notification == "Save ObservedHashtags")
                await SaveObservedHashtags().ConfigureAwait(false);
            else if (obj.Notification == "Delete ObservedHashtags")
                await DeleteObservedHashtags().ConfigureAwait(false);
        }

        private async void ReadMessage(NotificationMessage<string> obj)
        {
            if (obj.Notification == "Add ObservedUser")
            {
                var username = obj.Content;

                await DispatcherHelper.RunAsync(() =>
                {
                    ObservedUsers.Add(username);
                    ObservedUsers.Sort();
                });
                await WykopSDK.WykopSDK.LocalStorage.SaveObservedUsers(ObservedUsers);
            }
            else if(obj.Notification == "Remove ObservedUser")
            {
                var username = obj.Content;

                await DispatcherHelper.RunAsync(() => ObservedUsers.Remove(username));
                await WykopSDK.WykopSDK.LocalStorage.SaveObservedUsers(ObservedUsers);
            }
        }

        private async Task Logout()
        {
            await DispatcherHelper.RunAsync(() => ObservedHashtags.Clear());
            await DeleteObservedHashtags().ConfigureAwait(false);

            await DispatcherHelper.RunAsync(() => ObservedUsers.Clear());
            await WykopSDK.WykopSDK.LocalStorage.DeleteObservedUsers().ConfigureAwait(false);

            await WykopSDK.WykopSDK.LocalStorage.DeleteConversations().ConfigureAwait(false);
            await WykopSDK.WykopSDK.LocalStorage.DeleteBlacklists().ConfigureAwait(false);
        }

        #region ObservedHashtags
        private ObservableCollectionEx<string> _observedHashtags;
        public ObservableCollectionEx<string> ObservedHashtags
        {
            get { return _observedHashtags ?? (_observedHashtags = new ObservableCollectionEx<string>()); }
        }

        private async Task DownloadObservedHashtags()
        {
            var needToDownload = false;
            try
            {
                var file = await TempFolder.GetFileAsync("ObservedHashtags");
                var props = await file.GetBasicPropertiesAsync();
                if (DateTime.Now - props.DateModified > FileLifeSpan)
                {
                    needToDownload = true;
                }
                else
                {
                    var fileContent = await FileIO.ReadLinesAsync(file);
                    ObservedHashtags.Clear();
                    ObservedHashtags.AddRange(fileContent);
                }
            }
            catch (FileNotFoundException)
            {
                Logger.Error("ObservedHashtags not found.");
                needToDownload = true;
            }
            catch (Exception e)
            {
                Logger.Error("Couldn't read ObservedHashtags.", e);
                needToDownload = true;
            }

            if (needToDownload)
            {
                var data = await App.ApiService.GetUserObservedTags().ConfigureAwait(false);
                if (data != null)
                {
                    await DispatcherHelper.RunAsync(() =>
                    {
                        ObservedHashtags.Clear();
                        ObservedHashtags.AddRange(data);
                        data = null;
                    });

                    await SaveObservedHashtags().ConfigureAwait(false);
                }
            }
        }

        private async Task SaveObservedHashtags()
        {
            if (ObservedHashtags.Count == 0)
                return;

            try
            {
                var file = await TempFolder.CreateFileAsync("ObservedHashtags", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteLinesAsync(file, ObservedHashtags);
            }
            catch (Exception e)
            {
                Logger.Error("Couldn't save ObservedHashtags.", e);
            }
        }

        private async Task DeleteObservedHashtags()
        {
            try
            {
                var file = await TempFolder.GetFileAsync("ObservedHashtags");
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception e)
            {
                Logger.Error("Couldn't delete ObservedHashtags.", e);
            }
        }
        #endregion ObservedHashtags

        #region PopularHashtags
        private ObservableCollectionEx<string> _popularHashtags = null;
        public ObservableCollectionEx<string> PopularHashtags
        {
            get { return _popularHashtags ?? (_popularHashtags = new ObservableCollectionEx<string>()); }
        }

        private async Task DownloadPopularHashtags()
        {
            bool needToDownload = false;

            try
            {
                var file = await TempFolder.GetFileAsync("PopularHashtags");
                var props = await file.GetBasicPropertiesAsync();
                if (DateTime.Now - props.DateModified > FileLifeSpan)
                {
                    needToDownload = true;
                }
                else
                {
                    var fileContent = await FileIO.ReadLinesAsync(file);
                    Logger.Info(fileContent.Count + " entries in PopularHashtags");
                    if (fileContent.Count == 0)
                    {
                        needToDownload = true;
                    }
                    else
                    {
                        PopularHashtags.Clear();
                        PopularHashtags.AddRange(fileContent);
                    }
                }
            }
            catch(FileNotFoundException)
            {
                Logger.Error("PopularHashtags not found.");
                needToDownload = true;
            }
            catch (Exception e)
            {
                Logger.Error("Couldn't read PopularHashtags.", e);
                needToDownload = true;
            }

            if (needToDownload)
            {
                Logger.Info("Downloading PopularHashtags.");
                var data = await App.ApiService.GetPopularTags().ConfigureAwait(false);
                if (data != null)
                {
                    await DispatcherHelper.RunAsync(() =>
                    {
                        PopularHashtags.Clear();
                        PopularHashtags.AddRange(data.Select(x => x.HashtagName));
                        data = null;
                    });

                    await SavePopularHashtags().ConfigureAwait(false);
                }
                else
                {
                    Logger.Warn("Downloading PopularHashtags failed.");
                }
            } 
        }

        private async Task SavePopularHashtags()
        {
            try
            {
                var file = await TempFolder.CreateFileAsync("PopularHashtags", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteLinesAsync(file, PopularHashtags);
                Logger.Info("Saved PopularHashtags, " + PopularHashtags.Count + " entries.");
            }
            catch(Exception e)
            {
                Logger.Error("Couldn't save PopularHashtags.", e);
            }
          
        }
        #endregion

        #region Users
        private ObservableCollectionEx<string> _observedUsers = null;
        public ObservableCollectionEx<string> ObservedUsers
        {
            get { return _observedUsers ?? (_observedUsers = new ObservableCollectionEx<string>()); }
        }

        private ObservableCollectionEx<string> _tempUsers = null;
        public ObservableCollectionEx<string> TempUsers
        {
            get { return _tempUsers ?? (_tempUsers = new ObservableCollectionEx<string>()); }
        }
        #endregion

        #region Suggestions
        private ObservableCollectionEx<string> _suggestions = null;
        public ObservableCollectionEx<string> Suggestions
        {
            get { return _suggestions ?? (_suggestions = new ObservableCollectionEx<string>()); }
        }

        public void GenerateSuggestions(string input, bool hashtag)
        {
            Suggestions.Clear();

            if (hashtag)
            {
                var sugs = ObservedHashtags.Where(x => x.StartsWith(input)).
                    Concat(PopularHashtags.Where(x => x.StartsWith(input)));
                Suggestions.AddRange(sugs.Distinct());
            }
            else
            {
                var sugs = TempUsers.Where(x => x.StartsWith(input, StringComparison.CurrentCultureIgnoreCase)).
                    Concat(ObservedUsers.Where(x => x.StartsWith(input, StringComparison.CurrentCultureIgnoreCase)));
                Suggestions.AddRange(sugs.Distinct());
            }
        }
        #endregion
    }
}
