using System;
using System.Windows.Input;
using System.Linq.Expressions;
using GitHub.Models;
using GitHub.Services;
using GitHub.Extensions;

namespace GitHub.Commands
{
    /// <summary>
    /// A proxy <see cref="ICommand"/> that increments a usage counter after executing the command.
    /// </summary>
    public class UsageTrackingCommand : ICommand
    {
        readonly ICommand command;
        readonly Lazy<IUsageTracker> usageTracker;
        readonly Expression<Func<UsageModel.MeasuresModel, int>> counter;

        /// <summary>
        /// The usage tracker and counter to increment after the target command is executed.
        /// </summary>
        /// <param name="usageTracker">The usage tracker.</param>
        /// <param name="counter">The counter to increment.</param>
        /// <param name="command">The target command.</param>
        public UsageTrackingCommand(
            Lazy<IUsageTracker> usageTracker, Expression<Func<UsageModel.MeasuresModel, int>> counter,
            ICommand command)
        {
            this.command = command;
            this.usageTracker = usageTracker;
            this.counter = counter;
        }

        public event EventHandler CanExecuteChanged
        {
            add { command.CanExecuteChanged += value; }
            remove { command.CanExecuteChanged -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return command.CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            command.Execute(parameter);
            usageTracker.Value.IncrementCounter(counter).Forget();
        }
    }
}
