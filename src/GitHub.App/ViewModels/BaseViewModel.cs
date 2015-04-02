using GitHub.Validation;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GitHub.ViewModels
{
    public class BaseViewModel : ReactiveValidatableObject, IViewModel
    {
        public BaseViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        { }

        public ReactiveCommand<object> CancelCommand { get; protected set; }
        public ICommand Cancel { get { return CancelCommand; } }

        public string Title { get; protected set; }
    }
}
