using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Legacy;

public static class ReactiveTestHelper
{
    public static UserErrorResult OverrideHandlersForTesting()
    {
        return OverrideHandlersForTesting(RecoveryOptionResult.FailOperation);
    }

    public static UserErrorResult OverrideHandlersForTesting(RecoveryOptionResult recoveryOptionResult)
    {
        var subject = new Subject<UserError>();
        var handlerOverride = UserError.OverrideHandlersForTesting(err =>
        {
            subject.OnNext(err);
            return Observable.Return(recoveryOptionResult);
        });
        return new UserErrorResult(subject, handlerOverride);
    }

    public static bool CanExecute<TParam, TResult>(this ReactiveCommand<TParam, TResult> command, object parameter = null)
    {
        return ((ICommand)command).CanExecute(parameter);
    }
}

public sealed class UserErrorResult : IDisposable
{
    readonly IDisposable handlerOverride;

    public UserErrorResult(IObservable<UserError> userErrorObservable, IDisposable handlerOverride)
    {
        userErrorObservable.Subscribe(e => LastError = e);
        this.handlerOverride = handlerOverride;
    }

    public UserError LastError { get; set; }

    public void Dispose()
    {
        handlerOverride.Dispose();
    }
}
