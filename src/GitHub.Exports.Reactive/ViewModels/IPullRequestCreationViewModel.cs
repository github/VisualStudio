using GitHub.Models;
using System.Collections.Generic;

namespace GitHub.ViewModels
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IPullRequestCreationViewModel : IViewModel
    {
        IReadOnlyList<IBranch> Branches { get; }


    }
}
