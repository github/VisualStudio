using GitHub.Validation;
using NullGuard;
using ReactiveUI;
using System;
using System.Windows.Input;

namespace GitHub.ViewModels
{
    public class BaseViewModel : ReactiveValidatableObject, IViewModel
    {
        protected ObservableAsPropertyHelper<bool> isShowing;

        public BaseViewModel([AllowNull] IServiceProvider serviceProvider) : base(serviceProvider)
        { }

        public ReactiveCommand<object> CancelCommand { get; protected set; }
        public ICommand Cancel { get { return CancelCommand; } }

        public string Title { get; protected set; }
        public bool IsShowing { get { return isShowing.Value; } }
    }
}
