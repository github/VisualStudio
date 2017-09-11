using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.ViewModels;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class LoggedOutViewModelDesigner : PanePageViewModelBase, IViewModel
    {
        public LoggedOutViewModelDesigner()
        {
        }
    }
}