using System;
using GitHub.UI;

namespace GitHub.ViewModels.Dialog.Clone
{
    public interface IRepositoryItemViewModel
    {
        string Caption { get; }
        string Name { get; }
        string Owner { get; }
        Octicon Icon { get; }
        Uri Url { get; }
    }
}
