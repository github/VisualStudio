using System.ComponentModel.Composition;
using GitHub.UI;
using GitHub.Exports;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    public class GenericNoOriginRemoteView : ViewBase<INoOriginRemoteViewModel, GenericNoOriginRemoteView>
    {
    }

    [ExportViewFor(typeof(INoOriginRemoteViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class NoOriginRemoteView : GenericNoOriginRemoteView
    {
        public NoOriginRemoteView()
        {
            InitializeComponent();
        }
    }
}
