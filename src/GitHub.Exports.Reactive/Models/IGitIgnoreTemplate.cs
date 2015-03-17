using System.Threading.Tasks;
using GitHub.Exports;

namespace GitHub.Models
{
    public interface IGitIgnoreTemplate : ISelectable
    {
        string Name { get;  }
        string FilePath { get; }
        bool CanCopy { get; }
        Task CopyTo(string target, bool overwrite);
        bool IsRecommended { get; }
    }
}