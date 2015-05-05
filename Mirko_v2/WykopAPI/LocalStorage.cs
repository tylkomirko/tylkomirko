using MetroLog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using WykopAPI.Models;

namespace WykopAPI
{
    public class LocalStorage
    {
        private readonly TimeSpan FileLifeSpan = new TimeSpan(24, 0, 0);
        private readonly ILogger _log;

        private StorageFolder RootFolder = null;
        public Action InitAction = null;

        private List<uint> _entriesID = null;
        public List<uint> EntriesID
        {
            get { return _entriesID ?? (_entriesID = new List<uint>()); }
        }

        private List<Conversation> _conversations = null;
        public List<Conversation> Conversations
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
            var entriesFolder = await RootFolder.CreateFolderAsync("Entries", CreationCollisionOption.OpenIfExists);

            // get all files in entries folder
            var files = await entriesFolder.GetFilesAsync();
            EntriesID.Capacity = files.Count;
            foreach (var file in files)
                EntriesID.Add(Convert.ToUInt32(file.DisplayName));
        }

        public async Task<List<Conversation>> ReadConversations()
        {
            try
            {
                var file = await RootFolder.GetFileAsync("Conversations");
                var props = await file.GetBasicPropertiesAsync();
                if (DateTime.Now - props.DateModified > FileLifeSpan)
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
            catch (Exception)
            {
            }
        }
    }
}
