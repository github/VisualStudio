using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;

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
}

public class UserErrorResult : IDisposable
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
