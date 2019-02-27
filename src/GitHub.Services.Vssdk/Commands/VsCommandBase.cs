using System;
using System.ComponentModel.Design;
using System.Windows.Input;
using GitHub.Commands;
using Microsoft.VisualStudio.Shell;

#pragma warning disable CA1033 // Interface methods should be callable by child types

namespace GitHub.Services.Vssdk.Commands
{
    /// <summary>
    /// Base class for <see cref="VsCommand"/> and <see cref="VsCommand{TParam}"/>.
    /// </summary>
    public abstract class VsCommandBase : OleMenuCommand, IVsCommandBase
    {
        EventHandler canExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="VsCommandBase"/> class.
        /// </summary>
        /// <param name="commandSet">The GUID of the group the command belongs to.</param>
        /// <param name="commandId">The numeric identifier of the command.</param>
        protected VsCommandBase(Guid commandSet, int commandId)
            : base(ExecHandler, delegate { }, QueryStatusHandler, new CommandID(commandSet, commandId))
        {
        }

        /// <inheritdoc/>
        event EventHandler ICommand.CanExecuteChanged
        {
            add { canExecuteChanged += value; }
            remove { canExecuteChanged -= value; }
        }

        /// <inheritdoc/>
        bool ICommand.CanExecute(object parameter)
        {
            QueryStatus();
            return Enabled && Visible;
        }

        /// <inheritdoc/>
        void ICommand.Execute(object parameter)
        {
            ExecuteUntyped(parameter);
        }

        /// <summary>
        /// When overridden in a derived class, executes the command after casting the passed
        /// parameter to the correct type.
        /// </summary>
        /// <param name="parameter">The parameter</param>
        protected abstract void ExecuteUntyped(object parameter);

        protected override void OnCommandChanged(EventArgs e)
        {
            base.OnCommandChanged(e);
            canExecuteChanged?.Invoke(this, e);
        }

        protected virtual void QueryStatus()
        {
        }

        static void ExecHandler(object sender, EventArgs e)
        {
            var args = (OleMenuCmdEventArgs)e;
            var command = sender as VsCommandBase;
            command?.ExecuteUntyped(args.InValue);
        }

        static void QueryStatusHandler(object sender, EventArgs e)
        {
            var command = sender as VsCommandBase;
            command?.QueryStatus();
        }
    }
}
