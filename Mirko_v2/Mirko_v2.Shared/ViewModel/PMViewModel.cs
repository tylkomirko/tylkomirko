using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Text;
using WykopAPI.Models;

namespace Mirko_v2.ViewModel
{
    public class PMViewModel : ViewModelBase
    {
        public PM Data { get; set; }
        public EmbedViewModel EmbedVM { get; set; }
        public bool ShowArrow { get; set; } 

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
