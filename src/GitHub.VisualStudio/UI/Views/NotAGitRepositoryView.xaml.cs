using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;

namespace GitHub.VisualStudio.UI.Views
{
    public class GenericNotAGitRepositoryView : ViewBase<INotAGitRepositoryViewModel, GenericNotAGitRepositoryView>
    {
    }

    [ExportView(ViewType = UIViewType.NotAGitRepository)]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public partial class NotAGitRepositoryView : GenericNotAGitRepositoryView
    {
        public NotAGitRepositoryView()
        {
            this.InitializeComponent();
        }
    }
}