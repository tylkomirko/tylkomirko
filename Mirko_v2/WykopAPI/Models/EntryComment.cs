using Newtonsoft.Json;
using PropertyChanged;

namespace WykopAPI.Models
{
    [ImplementPropertyChanged]
    public class EntryComment : EntryBase
    {
        [JsonProperty("entry_id")]
        public uint EntryID { get; set; }

        /* Removed properties:
         public string author_avatar_big { get; set; }
         public string author_avatar_med { get; set; }
         public string author_avatar_lo { get; set; }
         public object source { get; set; }
         public string type { get; set; }
         public string app { get; set; }
         public string violation_url { get; set; }
         */
    }
}
