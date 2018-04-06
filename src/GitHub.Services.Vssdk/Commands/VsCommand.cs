using System;
using System.Windows.Input;
using GitHub.Commands;
using GitHub.Extensions;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Services.Vssdk.Commands
{
    /// <summary>
    /// Implements <see cref="ICommand"/> for <see cref="OleMenuCommand"/>s that don't accept a
    /// parameter.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class derives from <see cref="OleMenuCommand"/> and implements <see cref="ICommand"/>
    /// so that the command can be bound in the UI.
    /// </para>
    /// <para>
    /// To implement a new command, inherit from this class and override the <see cref="Execute"/>
    /// method to provide the implementation of the command.
    /// </para>
    /// </remarks>
    public abstract class VsCommand : VsCommandBase, IVsCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VsCommand"/> class.
        /// </summary>
        /// <param name="commandSet">The GUID of the group the command belongs to.</param>
        /// <param name="commandId">The numeric identifier of the command.</param>
        protected VsCommand(Guid commandSet, int commandId)
            : base(commandSet, commandId)
        {
        }

        /// <summary>
        /// Overridden by derived classes with the implementation of the command.
        /// </summary>
        /// <returns>A task that tracks the execution of the command.</returns>
        public abstract Task Execute();

        /// <inheritdoc/>
        protected sealed override void ExecuteUntyped(object parameter)
        {
            Execute().Forget();
        }
    }

    /// <summary>
    /// Implements <see cref="ICommand"/> for <see cref="OleMenuCommand"/>s that accept a parameter.
    /// </summary>
    /// <typeparam name="TParam">The type of the parameter accepted by the command.</typeparam>
    /// <para>
    /// This class derives from <see cref="OleMenuCommand"/> and implements <see cref="ICommand"/>
    /// so that the command can be bound in the UI.
    /// </para>
    /// <para>
    /// To implement a new command, inherit from this class and override the <see cref="Execute"/>
    /// method to provide the implementation of the command.
    /// </para>
    public abstract class VsCommand<TParam> : VsCommandBase, IVsCommand<TParam>, ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VsCommand"/> class.
        /// </summary>
        /// <param name="commandSet">The GUID of the group the command belongs to.</param>
        /// <param name="commandId">The numeric identifier of the command.</param>
        protected VsCommand(Guid commandSet, int commandId)
            : base(commandSet, commandId)
        {
        }

        /// <summary>
        /// Overridden by derived classes with the implementation of the command.
        /// </summary>
        /// /// <param name="parameter">The command parameter.</param>
        /// <returns>A task that tracks the execution of the command.</returns>
        public abstract Task Execute(TParam parameter);

        /// <inheritdoc/>
        protected sealed override void ExecuteUntyped(object parameter)
        {
            Execute((TParam)parameter).Forget();
        }
    }
}
