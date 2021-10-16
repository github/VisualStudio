using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    public class GenericLoggedOutView : ViewBase<ILoggedOutViewModel, GenericLoggedOutView>
    {
    }

    [ExportViewFor(typeof(ILoggedOutViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class LoggedOutView : GenericLoggedOutView
    {
        public LoggedOutView()
        {
            this.InitializeComponent();
            this.WhenActivated(d =>
            {
            });
        }
    }
}