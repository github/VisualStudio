using GitHub.Models;
using GitHub.UI;
using ReactiveUI;
using System;
using System.ComponentModel.Composition;
using System.Windows.Input;

namespace GitHub.ViewModels
{
    [Export(typeof(ICloneRepoViewModel))]
    public class CloneRepoViewModel : ICloneRepoViewModel
    {
        public ReactiveCommand<object> CancelCommand { get; private set; }
        public ICommand CancelCmd { get { return CancelCommand; } }
        public IObservable<object> Cancelling { get { return CancelCommand; } }

        public ReactiveCommand<object> OkCommand { get; private set; }
        public ICommand OkCmd { get { return OkCommand; } }

        [ImportingConstructor]
        public CloneRepoViewModel(IRepositoryHosts hosts)
        {
            
        }
    }
}
