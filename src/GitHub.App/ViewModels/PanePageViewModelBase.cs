using System.Windows.Input;
using ReactiveUI;
using NullGuard;
using GitHub.UI;

namespace GitHub.ViewModels
{
    public class PanePageViewModelBase : ReactiveObject, IPanePageViewModel
    {
        string title;

        public string Title
        {
            [return: AllowNull]
            get { return title; }
            protected set { this.RaiseAndSetIfChanged(ref title, value); }
        }

        public virtual void Initialize([AllowNull] ViewWithData data)
        {
        }
    }
}
