using System.Threading.Tasks;

namespace Mirko_v2.ViewModel
{
    interface IResumable
    {
        void SaveState(string pageName);
        void LoadState(string pageName);
        string GetName();
    }
}
