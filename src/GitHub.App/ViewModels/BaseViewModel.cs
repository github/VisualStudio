using System.Windows.Input;
using ReactiveUI;
using NullGuard;
using GitHub.UI;
using System;
using Octokit;

namespace GitHub.ViewModels
{
    public class BaseViewModel : ReactiveObject, IViewModel
    {
        protected ObservableAsPropertyHelper<bool> isShowing;

        public ReactiveCommand<object> CancelCommand { get; protected set; }
        public ICommand Cancel { get { return CancelCommand; } }

        public string Title {[return: AllowNull] get; protected set; }
        public bool IsShowing { get { return isShowing?.Value ?? true; } }

        public virtual void Initialize([AllowNull] ViewWithData data)
        {
        }

        protected virtual NewPullRequest GatherPullRequestInfo()
        {
            throw new NotImplementedException();
        }
    }
}
