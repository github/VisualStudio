using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using ReactiveUI;

namespace GitHub.VisualStudio.UI.Views
{
    public class GenericPullRequestDetailView : SimpleViewUserControl<IPullRequestDetailViewModel, GenericPullRequestDetailView>
    { }

    [ExportView(ViewType = UIViewType.PRDetail)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestDetailView : GenericPullRequestDetailView
    {
        public PullRequestDetailView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => ViewModel = e.NewValue as IPullRequestDetailViewModel;

            this.WhenActivated(d =>
            {
            });
        }

    }
}
