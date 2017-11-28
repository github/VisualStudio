using System.ComponentModel.Composition;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    [ExportViewFor(typeof(INavigationViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class NavigationView : UserControl
    {
        public NavigationView()
        {
            InitializeComponent();
        }
    }
}
