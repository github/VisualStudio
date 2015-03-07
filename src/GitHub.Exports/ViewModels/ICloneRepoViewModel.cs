using System;
using System.Collections.Generic;
using System.Windows.Input;
using GitHub.Models;

namespace GitHub.ViewModels
{
    public interface ICloneRepoViewModel : IViewModel
    {
        ICommand OkCommand { get; }
        ICommand CancelCommand { get; }
        IObservable<object> Cancelling { get; }
        ICollection<IRepositoryModel> Repositories { get; }
    }
}
