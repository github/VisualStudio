using System.Threading.Tasks;

namespace GitHub.InlineReviews.Services
{
    public interface IPullRequestStatusBarManager
    {
        /// <summary>
        /// Place the PR status control on Visual Studio's status bar.
        /// </summary>
        Task InitializeAsync();
    }
}
