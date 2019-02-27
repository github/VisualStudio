using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    public class GenericNotAGitHubRepositoryView : ViewBase<INotAGitHubRepositoryViewModel, GenericNotAGitHubRepositoryView>
    {
    }

    [ExportViewFor(typeof(INotAGitHubRepositoryViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class NotAGitHubRepositoryView : GenericNotAGitHubRepositoryView
    {
        public NotAGitHubRepositoryView()
        {
            this.InitializeComponent();
        }
    }
}