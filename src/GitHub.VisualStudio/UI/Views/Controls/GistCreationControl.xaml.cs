using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.UI;
using GitHub.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    public class GenericGistCreationControl : SimpleViewUserControl<IGistCreationViewModel, GistCreationControl>
    { }

    /// <summary>
    /// Interaction logic for GistCreationControl.xaml
    /// </summary>
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

                ViewModel.CreateGist.Subscribe(_ => NotifyDone());
            });
        }
    }
}
