using PropertyChanged;
using System.IO;

namespace WykopAPI.Models
{
    [ImplementPropertyChanged]
    public class NewEntry
    {
        public string AttachmentName { get; set; } 
        public string Text { get; set; }
        public string Embed { get; set; }

        public bool IsEditing { get; set; }
        public uint CommentID { get; set; }

        public uint EntryID { get; set; }

        public Stream FileStream { get; set; }
        public string FileName { get; set; }

        public void RemoveAttachment()
        {
            AttachmentName = null;
            Embed = null;
            FileName = null;

            if (FileStream != null)
                FileStream.Dispose();
            FileStream = null;
        }
    }
}
