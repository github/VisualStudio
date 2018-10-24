using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.UI;
using GitHub.ViewModels.Dialog;

namespace GitHub.VisualStudio.Views.Dialog
{
    public class GenericRepositoryRecloneView : ViewBase<IRepositoryRecloneViewModel, RepositoryRecloneView>
    {}

    /// <summary>
    /// Interaction logic for RepositoryRecloneView.xaml
    /// </summary>
    [ExportViewFor(typeof(IRepositoryRecloneViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class RepositoryRecloneView : GenericRepositoryRecloneView
    {
        public RepositoryRecloneView()
        {
            InitializeComponent();

            IsVisibleChanged += (s, e) =>
            {
                if (IsVisible)
                    this.TryMoveFocus(FocusNavigationDirection.First).Subscribe();
            };
        }
    }
}
