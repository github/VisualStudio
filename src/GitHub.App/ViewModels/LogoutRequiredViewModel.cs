using GitHub.Exports;
using System.ComponentModel.Composition;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.LogoutRequired)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LogoutRequiredViewModel : BaseViewModel, ILogoutRequiredViewModel
    {
    }
}
