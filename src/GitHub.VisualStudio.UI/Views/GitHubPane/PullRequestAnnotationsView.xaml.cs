using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using GitHub.Exports;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;
using UserControl = System.Windows.Controls.UserControl;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    [ExportViewFor(typeof(IPullRequestAnnotationsViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestAnnotationsView : UserControl
    {
        public PullRequestAnnotationsView()
        {
            InitializeComponent();
        }
    }
}
