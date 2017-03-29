using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;

namespace GitHub.VisualStudio.UI.Views
{
    public class GenericNotAGitHubRepositoryView : ViewBase<INotAGitHubRepositoryViewModel, GenericNotAGitHubRepositoryView>
    {
    }

    [ExportView(ViewType = UIViewType.NotAGitHubRepository)]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public partial class NotAGitHubRepositoryView : GenericNotAGitHubRepositoryView
    {
        public NotAGitHubRepositoryView()
        {
            this.InitializeComponent();
        }
    }
}