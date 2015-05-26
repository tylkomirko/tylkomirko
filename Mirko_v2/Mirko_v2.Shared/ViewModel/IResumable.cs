using System.Threading.Tasks;

namespace Mirko_v2.ViewModel
{
    interface IResumable
    {
        Task SaveState(string pageName);
        Task<bool> LoadState(string pageName);
        string GetName();
    }
}
