using PropertyChanged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WykopAPI.Models
{
    [ImplementPropertyChanged]
    public class NewEntry
    {
        public string AttachmentName { get; set; } 
        public string Text { get; set; }
        public string Embed { get; set; }

        public bool IsReply { get; set; }
        public bool IsEditing { get; set; }
        public uint CommentID { get; set; }

        public uint ID { get; set; }

        public Stream FileStream { get; set; }
        public string FileName { get; set; }
    }
}
