using System;
using GitHub.Exports;
using GitHub.UI;
using System.ComponentModel.Composition;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    public class GenericPullRequestCreationView : ViewBase<IPullRequestCreationViewModel, GenericPullRequestCreationView>
    { }

    [ExportViewFor(typeof(IPullRequestCreationViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestCreationView : GenericPullRequestCreationView
    {
        public PullRequestCreationView()
        {
            InitializeComponent();
        }
    }
}
