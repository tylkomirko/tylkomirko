using MetroLog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using WykopSDK.API.Models;

namespace WykopSDK.Storage
{
    public class LocalStorage
    {
        private readonly TimeSpan FileLifeSpan = new TimeSpan(24, 0, 0);
        private readonly ILogger _log = null;

        private StorageFolder RootFolder = null;
        public Action InitAction = null;

        private List<Conversation> _conversations = null;
        public  List<Conversation> Conversations
        {
            get { return _conversations ?? (_conversations = new List<Conversation>()); }
            set { _conversations = value; }
        }

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
                using (var stream = await file.OpenStreamForReadAsync())
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
                _log.Error("Something went wrong reading Conversations: ", e);
                return null;
            }
        }

        public async Task SaveConversations(IEnumerable<Conversation> conversations)
        {
            try
            {
                var file = await RootFolder.CreateFileAsync("Conversations", CreationCollisionOption.ReplaceExisting);

                using (var stream = await file.OpenStreamForWriteAsync())
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
                _log.Error("Something went wrong saving Conversations: ", e);
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
                _log.Error("Something went wrong deleting Conversations: ", e);
            }
        }
    }
}
