using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Mirko_v2.Utils;
using Newtonsoft.Json;
using WykopAPI.Models;

namespace Mirko_v2.ViewModel
{
    public class CommentViewModel : EntryBaseViewModel
    {
        public EntryComment Data { get; set; }

        public CommentViewModel()
        {
        }

        public CommentViewModel(EntryComment c) : base(c)
        {
            Data = c;
            Data.Embed = null;
            c = null;
        }
    }
}
