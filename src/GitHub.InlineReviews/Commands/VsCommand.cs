using System;
using System.Windows.Input;
using GitHub.Extensions;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GitHub.InlineReviews.Commands
{
    /// <summary>
    /// Wraps a Visual Studio <see cref="OleMenuCommand"/> for commands that don't accept a parameter.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class wraps an <see cref="OleMenuCommand"/> and also implements <see cref="ICommand"/>
    /// so that the command can be bound to in the UI.
    /// </para>
    /// <para>
    /// To implement a new command, inherit from this class and add an <see cref="ExportCommandAttribute"/>
    /// to the class with the type of the package that the command is registered by. You can then override
    /// the <see cref="Execute"/> method to provide the implementation of the command.
    /// </para>
    /// <para>
    /// Commands are registered by a package on initialization by calling
    /// <see cref="RegisterPackageCommands{TPackage}(TPackage)"/>.
    /// </para>
    /// </remarks>
    abstract class VsCommand : VsCommandBase, IVsCommand
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

        /// <inheritdoc/>
        public override void Register(IServiceProvider package)
        {
            var command = new OleMenuCommand(
                (s, e) => Execute().Forget(),
                (s, e) => { },
                (s, e) => BeforeQueryStatus((OleMenuCommand)s),
                VsCommandID);
            Register(package, command);
        }

        /// <summary>
        /// Overridden by derived classes with the implementation of the command.
        /// </summary>
        /// <returns>A task that tracks the execution of the command.</returns>
        public abstract Task Execute();

        /// <inheritdoc/>
        protected override void ExecuteUntyped(object parameter)
        {
            Execute().Forget();
        }
    }

    /// <summary>
    /// Wraps a Visual Studio <see cref="OleMenuCommand"/> for commands that accept a parameter.
    /// </summary>
    /// <typeparam name="TParam">The type of the parameter accepted by the command.</typeparam>
    /// <remarks>
    /// <para>
    /// To implement a new command, inherit from this class and add an <see cref="ExportCommandAttribute"/>
    /// to the class with the type of the package that the command is registered by. You can then override
    /// the <see cref="Execute"/> method to provide the implementation of the command.
    /// </para>
    /// <para>
    /// Commands are registered by a package on initialization by calling
    /// <see cref="RegisterPackageCommands{TPackage}(TPackage)"/>.
    /// </para>
    /// </remarks>
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

        /// <inheritdoc/>
        public override void Register(IServiceProvider package)
        {
            var command = new OleMenuCommand(
                (s, e) => Execute((TParam)((OleMenuCmdEventArgs)e).InValue).Forget(),
                (s, e) => { },
                (s, e) => BeforeQueryStatus((OleMenuCommand)s),
                VsCommandID);
            Register(package, command);
        }

        /// <summary>
        /// Overridden by derived classes with the implementation of the command.
        /// </summary>
        /// /// <param name="parameter">The command parameter.</param>
        /// <returns>A task that tracks the execution of the command.</returns>
        public abstract Task Execute(TParam parameter);

        /// <inheritdoc/>
        protected override void ExecuteUntyped(object parameter)
        {
            Execute((TParam)parameter).Forget();
        }
    }
}
