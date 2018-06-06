using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;

public static class CommandTestHelpers
{
    public static IObservable<Unit> ExecuteAsync(this ICommand command)
    {
        var reactiveCommand = command as ReactiveCommand<object>;
        if (reactiveCommand != null)
            return reactiveCommand.ExecuteAsync().Select(_ => Unit.Default);

        var unitCommand = command as ReactiveCommand<Unit>;
        if (unitCommand != null)
            return unitCommand.ExecuteAsync();

        return Observable.Start(() => command.Execute(null));
    }
}