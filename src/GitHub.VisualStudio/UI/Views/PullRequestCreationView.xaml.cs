using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using ReactiveUI;

namespace GitHub.VisualStudio.UI.Views
{
    public class GenericPullRequestCreationView : SimpleViewUserControl<IPullRequestCreationViewModel, GenericPullRequestCreationView>
    { }

    [ExportView(ViewType = UIViewType.PRCreation)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestCreationView : GenericPullRequestCreationView
    {
        public PullRequestCreationView()
        {
            InitializeComponent();
            // DataContextChanged += (s, e) => ViewModel = e.NewValue as IPullRequestCreationViewModel;
            DataContextChanged += (s, e) => ViewModel = new GitHub.SampleData.PullRequestCreationViewModelDesigner() as IPullRequestCreationViewModel;

            this.WhenActivated(d =>
            {
            });
        }
    }
}
