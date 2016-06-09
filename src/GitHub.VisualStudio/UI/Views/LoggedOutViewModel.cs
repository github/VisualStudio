using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.VisualStudio.UI.Views
{
    [ExportViewModel(ViewType = UIViewType.LoggedOut)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoggedOutViewModel : BaseViewModel
    {
        public LoggedOutViewModel()
        {
        }
    }
}