using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    public class GenericNotAGitRepositoryView : ViewBase<INotAGitRepositoryViewModel, GenericNotAGitRepositoryView>
    {
    }

    [ExportViewFor(typeof(INotAGitRepositoryViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public partial class NotAGitRepositoryView : GenericNotAGitRepositoryView
    {
        public NotAGitRepositoryView()
        {
            this.InitializeComponent();
        }
    }
}