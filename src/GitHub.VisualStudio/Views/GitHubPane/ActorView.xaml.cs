using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    [ExportViewFor(typeof(IActorViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ActorView : UserControl
    {
        [ImportingConstructor]
        public ActorView()
        {
            InitializeComponent();
        }
    }
}
