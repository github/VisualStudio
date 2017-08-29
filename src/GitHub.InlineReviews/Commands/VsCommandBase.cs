using System;
using System.ComponentModel.Design;
using System.Windows.Input;
using Microsoft.VisualStudio.Shell;

namespace GitHub.InlineReviews.Commands
{
    /// <summary>
    /// Base class for <see cref="VsCommand"/> and <see cref="VsCommand{TParam}"/>.
    /// </summary>
    abstract class VsCommandBase : IVsCommandBase
    {
        EventHandler canExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="VsCommandBase"/> class.
        /// </summary>
        /// <param name="commandSet">The GUID of the group the command belongs to.</param>
        /// <param name="commandId">The numeric identifier of the command.</param>
        protected VsCommandBase(Guid commandSet, int commandId)
        {
            VsCommandID = new CommandID(commandSet, commandId);
        }

        /// <summary>
        /// Gets a value indicating whether the command is enabled.
        /// </summary>
        public virtual bool IsEnabled => true;

        /// <summary>
        /// Gets a value indicating whether the command is visible.
        /// </summary>
        public virtual bool IsVisible => true;

        /// <inheritdoc/>
        event EventHandler ICommand.CanExecuteChanged
        {
            add { canExecuteChanged += value; }
            remove { canExecuteChanged -= value; }
        }

        /// <inheritdoc/>
        bool ICommand.CanExecute(object parameter)
        {
            return IsEnabled && IsVisible;
        }

        /// <inheritdoc/>
        void ICommand.Execute(object parameter)
        {
            ExecuteUntyped(parameter);
        }

        /// <inheritdoc/>
        public abstract void Register(IServiceProvider package);

        /// <summary>
        /// Gets the package that registered the command.
        /// </summary>
        protected IServiceProvider Package { get; private set; }

        /// <summary>
        /// Gets the group and identifier for the command.
        /// </summary>
        protected CommandID VsCommandID { get; }

        /// <summary>
        /// Implements the event handler for <see cref="OleMenuCommand.BeforeQueryStatus"/>.
        /// </summary>
        /// <param name="command">The event parameter.</param>
        protected void BeforeQueryStatus(OleMenuCommand command)
        {
            command.Enabled = IsEnabled;
            command.Visible = IsVisible;
        }

        /// <summary>
        /// When overridden in a derived class, executes the command after casting the passed
        /// parameter to the correct type.
        /// </summary>
        /// <param name="parameter">The parameter</param>
        protected abstract void ExecuteUntyped(object parameter);

        /// <summary>
        /// Registers an <see cref="OleMenuCommand"/> with a package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="command">The command.</param>
        protected void Register(IServiceProvider package, OleMenuCommand command)
        {
            Package = package;
            var serviceProvider = (IServiceProvider)package;
            var mcs = (IMenuCommandService)serviceProvider.GetService(typeof(IMenuCommandService));
            mcs?.AddCommand(command);
        }
    }
}
