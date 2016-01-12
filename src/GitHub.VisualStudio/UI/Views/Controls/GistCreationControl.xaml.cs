using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using ReactiveUI;
using System;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    public class GenericGistCreationControl : SimpleViewUserControl<IGistCreationViewModel, GistCreationControl>
    { }

    [ExportView(ViewType=UIViewType.Gist)]
    [PartCreationPolicy(CreationPolicy.NonShared)] 
    public partial class GistCreationControl
    {
        public GistCreationControl()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                d(this.Bind(ViewModel, vm => vm.Description, v => v.descriptionTextBox.Text));
                d(this.Bind(ViewModel, vm => vm.FileName, v => v.fileNameTextBox.Text));
                d(this.Bind(ViewModel, vm => vm.IsPrivate, v => v.makePrivate.IsChecked));
                d(this.BindCommand(ViewModel, vm => vm.CreateGist, v => v.createGistButton));

                d(this.Bind(ViewModel, vm => vm.Account, v => v.accountStackPanel.DataContext));

                ViewModel.CreateGist.Subscribe(_ => NotifyDone());
            });
        }
    }
}
