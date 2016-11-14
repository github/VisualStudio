using System.Threading.Tasks;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;
using GitHub.Extensions;

namespace GitHub.Helpers
{
    public static class ThreadingHelper
    {
        public async static Task SwitchToMainThreadAsync()
        {
            if (!Guard.InUnitTestRunner())
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        }
    }
}
