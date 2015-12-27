using MetroLog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using WykopSDK.API.Models;
using System.Linq;

namespace WykopSDK.Storage
{
    public class LocalStorage
    {
        private readonly TimeSpan FileLifeSpan = new TimeSpan(24, 0, 0);
        private readonly ILogger _log = null;

        private StorageFolder RootFolder = null;
        public Action InitAction = null;

        public LocalStorage()
        {
            InitAction = new Action(async () => await Init());
            _log = LogManagerFactory.DefaultLogManager.GetLogger<LocalStorage>();
        }

        public async Task Init()
        {
            var tempFolder = ApplicationData.Current.TemporaryFolder; // maybe change to LocalFolder?
            RootFolder = await tempFolder.CreateFolderAsync("Storage", CreationCollisionOption.OpenIfExists);
        }

        #region Conversations
        public async Task<List<Conversation>> ReadConversations()
        {
            try
            {
                if (RootFolder == null)
                    await Init();

                var file = await RootFolder.GetFileAsync("Conversations");
                var props = await file.GetBasicPropertiesAsync();
                if (DateTime.Now - props.DateModified > new TimeSpan(6, 0, 0))
                    return null;

                _log.Info("Reading Conversations.");
                using (var stream = await file.OpenStreamForReadAsync().ConfigureAwait(false))
                using (StreamReader sr = new StreamReader(stream))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return serializer.Deserialize<List<Conversation>>(reader);
                }
            }
            catch(FileNotFoundException)
            {
                _log.Error("Conversations not found.");
                return null;
            }
            catch (Exception e)
            {
                _log.Error("Reading Conversations: ", e);
                return null;
            }
        }

        public async Task SaveConversations(IEnumerable<Conversation> conversations)
        {
            try
            {
                var file = await RootFolder.CreateFileAsync("Conversations", CreationCollisionOption.ReplaceExisting);

                using (var stream = await file.OpenStreamForWriteAsync().ConfigureAwait(false))
                using (StreamWriter sw = new StreamWriter(stream))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, conversations);
                }

                _log.Info("Saved Conversations.");
            }
            catch (Exception e)
            {
                _log.Error("Saving Conversations: ", e);
            }
            finally
            {
                conversations = null;
            }
        }

        public async Task DeleteConversations()
        {
            try
            {
                var file = await RootFolder.GetFileAsync("Conversations");
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);

                _log.Info("Deleted Conversations.");
            }
            catch (Exception e)
            {
                _log.Error("Deleting Conversations: ", e);
            }
        }
        #endregion

        #region Blacklists
        public async Task<List<string>> ReadBlacklistedTags()
        {
            try
            {
                if (RootFolder == null)
                    await Init();

                var file = await RootFolder.GetFileAsync("BlacklistedTags");
                var props = await file.GetBasicPropertiesAsync();
                if (DateTime.Now - props.DateModified > new TimeSpan(6, 0, 0))
                    return null;

                _log.Info("Reading BlacklistedTags.");
                var ilist = await FileIO.ReadLinesAsync(file);
                return ilist.ToList();
            }
            catch (Exception e)
            {
                _log.Error("ReadBlacklistedTags", e);
                return null;
            }
        }

        public async Task<List<string>> ReadBlacklistedUsers()
        {
            try
            {
                if (RootFolder == null)
                    await Init();

                var file = await RootFolder.GetFileAsync("BlacklistedUsers");
                var props = await file.GetBasicPropertiesAsync();
                if (DateTime.Now - props.DateModified > new TimeSpan(6, 0, 0))
                    return null;

                _log.Info("Reading BlacklistedUsers.");
                var ilist = await FileIO.ReadLinesAsync(file);
                return ilist.ToList();
            }
            catch (Exception e)
            {
                _log.Error("BlacklistedUsers", e);
                return null;
            }
        }

        public async Task SaveBlacklists(IEnumerable<string> tags = null, IEnumerable<string> users = null)
        {
            if (tags != null)
            {
                try
                {
                    var file = await RootFolder.CreateFileAsync("BlacklistedTags", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteLinesAsync(file, tags);
                }
                catch (Exception e)
                {
                    _log.Error("Saving BlacklistedTags: ", e);
                }
            }

            if (users != null)
            {
                try
                {
                    var file = await RootFolder.CreateFileAsync("BlacklistedUsers", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteLinesAsync(file, users);
                }
                catch (Exception e)
                {
                    _log.Error("Saving BlacklistedUsers: ", e);
                }
            }
        }

        public async Task DeleteBlacklists()
        {
            try
            {
                var file = await RootFolder.GetFileAsync("BlacklistedTags");
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception e)
            {
                _log.Error("Deleting BlacklistedTags: ", e);
            }

            try
            {
                var file = await RootFolder.GetFileAsync("BlacklistedUsers");
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception e)
            {
                _log.Error("Deleting BlacklistedUsers: ", e);
            }
        }
        #endregion

        #region ObservedUsers
        public async Task<List<string>> ReadObservedUsers()
        {
            try
            {
                if (RootFolder == null)
                    await Init();

                var file = await RootFolder.GetFileAsync("ObservedUsers");
                var props = await file.GetBasicPropertiesAsync();
                if (DateTime.Now - props.DateModified > new TimeSpan(6, 0, 0))
                    return null;

                _log.Info("Reading ObservedUsers.");
                var ilist = await FileIO.ReadLinesAsync(file);
                return ilist.ToList();
            }
            catch (Exception e)
            {
                _log.Error("ObservedUsers", e);
                return null;
            }
        }

        public async Task SaveObservedUsers(IEnumerable<string> users)
        {
            if (users != null)
            {
                try
                {
                    var file = await RootFolder.CreateFileAsync("ObservedUsers", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteLinesAsync(file, users);
                }
                catch (Exception e)
                {
                    _log.Error("Saving ObservedUsers: ", e);
                }
            }
        }

        public async Task DeleteObservedUsers()
        {
            try
            {
                var file = await RootFolder.GetFileAsync("ObservedUsers");
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception e)
            {
                _log.Error("Deleting ObservedUsers: ", e);
            }
        }
        #endregion
    }
}
