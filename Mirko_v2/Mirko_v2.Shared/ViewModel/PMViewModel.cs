using GalaSoft.MvvmLight;
using WykopSDK.API.Models;

namespace Mirko_v2.ViewModel
{
    public class PMViewModel : ViewModelBase
    {
        public PM Data { get; set; }
        public EmbedViewModel EmbedVM { get; set; }
        public bool ShowArrow { get; set; } 

        public PMViewModel()
        {
        }

        public PMViewModel(PM p)
        {
            Data = p;
            if(Data.Embed != null)
                EmbedVM = new EmbedViewModel(Data.Embed);
            Data.Embed = null;
            p = null;
        }
    }
}
