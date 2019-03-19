using System;
using System.ComponentModel.Composition;

namespace GitHub.ViewModels
{
    /// <summary>
    /// View model which displays a spinner.
    /// </summary>
    [Export(typeof(ISpinnerViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SpinnerViewModel : ViewModelBase, ISpinnerViewModel
    {
    }
}
