using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using System.ComponentModel.Composition;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    public class GenericLogoutRequiredControl : SimpleViewUserControl<ILogoutRequiredViewModel, LogoutRequiredControl>
    { }

    /// <summary>
    /// Interaction logic for LogoutRequiredControl.xaml
    /// </summary>
    [ExportView(ViewType = UIViewType.LogoutRequired)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class LogoutRequiredControl : GenericLogoutRequiredControl
    {
        public LogoutRequiredControl()
        {
        }
    }
}
