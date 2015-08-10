using System.Threading.Tasks;

namespace Mirko.ViewModel
{
    interface IResumable
    {
        Task SaveState(string pageName);
        Task<bool> LoadState(string pageName);
        string GetName();
    }
}
