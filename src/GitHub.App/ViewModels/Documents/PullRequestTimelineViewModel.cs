using System;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.App.ViewModels.Documents
{
    public class PullRequestTimelineViewModel
    {
        public IReadOnlyReactiveList<IViewModel> Items { get; }
    }
}
