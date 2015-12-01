using GitHub.Exports;
using System.ComponentModel.Composition;
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

        public ReactiveCommand<object> CreatePublicCommand { get; }
        public ReactiveCommand<object> CreatePrivateCommand { get; }
        public string Description { get; }
        public string Content { get; }
        public string FileName { get; }
    }
}
