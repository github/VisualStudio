using Microsoft.VisualStudio.TextManager.Interop;

namespace GitHub.Services
{
    public interface INavigationService
    {
        IVsTextView FindActiveView();
        IVsTextView NavigateToEquivalentPosition(IVsTextView sourceView, string targetFile);
    }
}