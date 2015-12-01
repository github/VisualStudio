using GitHub.Exports;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Gist)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GistCreationViewModel : BaseViewModel, IGistCreationViewModel
    {
        [ImportingConstructor]
        GistCreationViewModel()
        {
            Title = Resources.CreateGistTitle;
        }

        public ReactiveCommand<object> CreateCommand { get; }
        public bool IsPublic { get; }
        public string Description { get; }
        public string Content { get; }
        public string FileName { get; }
    }
}
