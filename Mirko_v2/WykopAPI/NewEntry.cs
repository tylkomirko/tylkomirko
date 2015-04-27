using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WykopAPI
{
    public class NewEntry
    {
        public string body { get; set; }
        public string embed { get; set; }

        public bool isEditing { get; set; }
        public bool isReply { get; set; }
        public uint commentID { get; set; }

        public uint ID { get; set; }
        //public EntriesCollectionType type { get; set; }

        public Stream fileStream { get; set; }
        public string fileName { get; set; }
    }
}
