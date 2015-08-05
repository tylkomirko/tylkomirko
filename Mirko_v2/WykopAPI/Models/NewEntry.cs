using Newtonsoft.Json;
using PropertyChanged;
using Windows.Storage;

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

        [JsonIgnore]
        public StorageFile[] Files { get; set; }

        public void RemoveAttachment()
        {
            AttachmentName = null;
            Embed = null;
            Files = null;
        }

        public void SetAttachmentName(int count) // used when sending multiple attachments. count > 1.
        {
            if (count == 2 || count == 3 || count == 4)
                AttachmentName = count + " pliki";
            else
                AttachmentName = count + " plików";
        }
    }
}
