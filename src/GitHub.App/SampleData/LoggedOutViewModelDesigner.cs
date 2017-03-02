using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Windows.Input;
using GitHub.Collections;
using GitHub.Models;
using GitHub.ViewModels;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class LoggedOutViewModelDesigner : DialogViewModelBase, IViewModel
    {
        public LoggedOutViewModelDesigner()
        {
        }
    }
}