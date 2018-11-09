using System.ComponentModel.Composition;
using GitHub.UI;
using GitHub.Exports;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    public class GenericNoRemoteOriginView : ViewBase<INoRemoteOriginViewModel, GenericNoRemoteOriginView>
    {
    }

    [ExportViewFor(typeof(INoRemoteOriginViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class NoRemoteOriginView : GenericNoRemoteOriginView
    {
        public NoRemoteOriginView()
        {
            InitializeComponent();
        }
    }
}
