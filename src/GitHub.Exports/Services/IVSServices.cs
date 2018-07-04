using Microsoft.VisualStudio;

namespace GitHub.Services
{
    public interface IVSServices
    {
        string VSVersion { get; }
        bool TryOpenRepository(string directory);
        VSConstants.MessageBoxResult ShowMessageBoxInfo(string message);
    }
}