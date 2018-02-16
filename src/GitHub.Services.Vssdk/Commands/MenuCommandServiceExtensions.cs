using System;
using System.ComponentModel.Design;
using System.Windows.Input;
using GitHub.Commands;
using GitHub.Extensions;
using Microsoft.VisualStudio.Shell;

namespace GitHub.Services.Vssdk.Commands
{
    /// <summary>
    /// Extension methods for <see cref="IMenuCommandService"/>.
    /// </summary>
    public static class MenuCommandServiceExtensions
    {
        /// <summary>
        /// Adds an <see cref="IVsCommand"/> or <see cref="IVsCommand{TParam}"/> to a menu.
        /// </summary>
        /// <param name="service">The menu command service.</param>
        /// <param name="command">The command to add.</param>
        public static void AddCommand(this IMenuCommandService service, IVsCommandBase command)
        {
            Guard.ArgumentNotNull(service, nameof(service));
            Guard.ArgumentNotNull(command, nameof(command));

            service.AddCommand((MenuCommand)command);
        }

        /// <summary>
        /// Binds an <see cref="ICommand"/> to a Visual Studio command.
        /// </summary>
        /// <param name="service">The menu command service.</param>
        /// <param name="id">The ID of the visual studio command.</param>
        /// <param name="command">The <see cref="ICommand"/> to bind</param>
        /// <param name="hideWhenDisabled">
        /// If true, the visual studio command will be hidden when disabled.
        /// </param>
        /// <remarks>
        /// This method wires up the <paramref name="command"/> to be executed when the Visual Studio
        /// command is invoked, and for the <paramref name="command"/>'s
        /// <see cref="ICommand.CanExecute(object)"/> state to control the enabled/visible state of
        /// the Visual Studio command.
        /// </remarks>
        /// <returns>
        /// The created <see cref="OleMenuCommand"/>.
        /// </returns>
        public static OleMenuCommand BindCommand(
            this IMenuCommandService service,
            CommandID id,
            ICommand command,
            bool hideWhenDisabled = false)
        {
            Guard.ArgumentNotNull(service, nameof(service));
            Guard.ArgumentNotNull(id, nameof(id));
            Guard.ArgumentNotNull(command, nameof(command));

            var bound = new BoundCommand(id, command, hideWhenDisabled);
            service.AddCommand(bound);
            return bound;
        }

        class BoundCommand : OleMenuCommand
        {
            readonly ICommand inner;
            readonly bool hideWhenDisabled;

            public BoundCommand(CommandID id, ICommand command, bool hideWhenDisabled)
                : base(InvokeHandler, delegate { }, HandleBeforeQueryStatus, id)
            {
                Guard.ArgumentNotNull(id, nameof(id));
                Guard.ArgumentNotNull(command, nameof(command));

                inner = command;
                this.hideWhenDisabled = hideWhenDisabled;
                inner.CanExecuteChanged += (s, e) => HandleBeforeQueryStatus(this, e);
            }

            static void InvokeHandler(object sender, EventArgs e)
            {
                var command = sender as BoundCommand;
                command?.inner.Execute((e as OleMenuCmdEventArgs)?.InValue);
            }

            static void HandleBeforeQueryStatus(object sender, EventArgs e)
            {
                var command = sender as BoundCommand;

                if (command != null)
                {
                    command.Enabled = command.inner.CanExecute(null);
                    command.Visible = command.hideWhenDisabled ? command.Enabled : true;
                }
            }
        }
    }
}
