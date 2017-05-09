using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.VisualStudio.UI.Views
{
    public class GenericLoggedOutView : ViewBase<ILoggedOutViewModel, GenericLoggedOutView>
    {
    }

    [ExportView(ViewType = UIViewType.LoggedOut)]
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