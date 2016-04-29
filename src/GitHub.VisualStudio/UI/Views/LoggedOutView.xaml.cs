using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.VisualStudio.UI.Views
{
    public class GenericLoggedOutView : SimpleViewUserControl<IViewModel, GenericLoggedOutView>
    {
    }

    [ExportView(ViewType = UIViewType.LoggedOut)]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public partial class LoggedOutView : GenericLoggedOutView
    {
        public LoggedOutView()
        {
            this.InitializeComponent();
            DataContextChanged += (s, e) => ViewModel = e.NewValue as LoggedOutViewModel;
            this.WhenActivated(d =>
            {
            });
        }
    }
}