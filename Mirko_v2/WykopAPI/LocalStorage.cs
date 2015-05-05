using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace WykopAPI
{
    public class LocalStorage
    {
        private List<uint> _entriesID = null;
        public List<uint> EntriesID
        {
            get { return _entriesID ?? (_entriesID = new List<uint>()); }
        }

        public async Task Init()
        {
            var rootFolder = ApplicationData.Current.TemporaryFolder; // maybe change to LocalFolder?
            var folder = await rootFolder.CreateFolderAsync("Storage", CreationCollisionOption.OpenIfExists);
            var entriesFolder = await folder.CreateFolderAsync("Entries", CreationCollisionOption.OpenIfExists);

            // get all files in entries folder
            var files = await entriesFolder.GetFilesAsync();
            EntriesID.Capacity = files.Count;
            foreach (var file in files)
                EntriesID.Add(Convert.ToUInt32(file.DisplayName));
        }
    }
}
