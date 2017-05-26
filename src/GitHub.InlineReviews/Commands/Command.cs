using System;
using System.ComponentModel.Design;
using GitHub.Extensions;
using GitHub.Services;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GitHub.InlineReviews.Commands
{
    /// <summary>
    /// Wraps a Visual Studio <see cref="OleMenuCommand"/> for commands that don't accept a parameter.
    /// </summary>
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
    abstract class Command : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="commandSet">The GUID of the group the command belongs to.</param>
        /// <param name="commandId">The numeric identifier of the command.</param>
        protected Command(Guid commandSet, int commandId)
        {
            CommandID = new CommandID(commandSet, commandId);
        }

        /// <summary>
        /// Gets a value indicating whether the command is enabled.
        /// </summary>
        protected virtual bool IsEnabled => true;

        /// <summary>
        /// Gets a value indicating whether the command is visible.
        /// </summary>
        protected virtual bool IsVisible => true;

        /// <summary>
        /// Gets the group and identifier for the command.
        /// </summary>
        protected CommandID CommandID { get; }

        /// <summary>
        /// Gets the package that registered the command.
        /// </summary>
        protected IServiceProvider Package { get; private set; }

        /// <summary>
        /// Registers the command with a package.
        /// </summary>
        /// <param name="package">The package registering the command.</param>
        /// <remarks>
        /// This method should not be called directly, instead packages should call
        /// <see cref="RegisterPackageCommands{TPackage}(TPackage)"/> on initialization.
        /// </remarks>
        public virtual void Register(IServiceProvider package)
        {
            var command = new OleMenuCommand(
                (s, e) => Execute().Forget(),
                (s, e) => { },
                (s, e) => BeforeQueryStatus((OleMenuCommand)s),
                CommandID);
            Register(package, command);
        }

        /// <summary>
        /// Registers the commands for a package.
        /// </summary>
        /// <typeparam name="TPackage">The type of the package.</typeparam>
        /// <param name="package">The package.</param>
        public static void RegisterPackageCommands<TPackage>(TPackage package) where TPackage : Package
        {
            var serviceProvider = package.GetServiceSafe<IGitHubServiceProvider>();
            var commands = serviceProvider?.ExportProvider?.GetExports<ICommand, IExportCommandMetadata>();

            if (commands != null)
            {
                foreach (var command in commands)
                {
                    if (command.Metadata.PackageType == typeof(TPackage))
                    {
                        command.Value.Register(package);
                    }
                }
            }
        }

        /// <summary>
        /// Overridden by derived classes with the implementation of the command.
        /// </summary>
        /// <returns>A task that tracks the execution of the command.</returns>
        protected abstract Task Execute();

        internal void BeforeQueryStatus(OleMenuCommand command)
        {
            command.Enabled = IsEnabled;
            command.Visible = IsVisible;
        }

        internal void Register(IServiceProvider package, OleMenuCommand command)
        {
            Package = package;
            var serviceProvider = (IServiceProvider)package;
            var mcs = (IMenuCommandService)serviceProvider.GetService(typeof(IMenuCommandService));
            mcs?.AddCommand(command);
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
    abstract class Command<TParam> : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="commandSet">The GUID of the group the command belongs to.</param>
        /// <param name="commandId">The numeric identifier of the command.</param>
        protected Command(Guid commandSet, int commandId)
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
                CommandID);
            Register(package, command);
        }

        /// <summary>
        /// Seals the method as the <see cref="Execute(TParam)"/> method should be overridden instead.
        /// </summary>
        /// <returns></returns>
        protected sealed override Task Execute() => Task.CompletedTask;

        /// <summary>
        /// Overridden by derived classes with the implementation of the command.
        /// </summary>
        /// <param name="param">The command parameter.</param>
        /// <returns>A task that tracks the execution of the command.</returns>
        protected abstract Task Execute(TParam param);
    }
}
