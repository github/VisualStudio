using System;
using System.ComponentModel.Design;
using GitHub.Extensions;
using GitHub.Services;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GitHub.InlineReviews.Commands
{
    abstract class Command : ICommand
    {
        protected Command(Guid commandSet, int commandId)
        {
            CommandID = new CommandID(commandSet, commandId);
        }

        protected virtual bool Enabled => true;
        protected virtual bool Visible => true;
        protected CommandID CommandID { get; }
        protected Package Package { get; private set; }
        protected IGitHubServiceProvider GitHubServiceProvider => Package.GetServiceSafe<IGitHubServiceProvider>();

        public virtual void Register(Package package)
        {
            var command = new OleMenuCommand(
                (s, e) => Execute().Forget(),
                (s, e) => { },
                (s, e) => BeforeQueryStatus((OleMenuCommand)s),
                CommandID);
            Register(package, command);
        }

        public static void RegisterPackageCommands(Package package)
        {
            var serviceProvider = package.GetServiceSafe<IGitHubServiceProvider>();
            var commands = serviceProvider?.ExportProvider?.GetExportedValues<ICommand>();

            if (commands != null)
            {
                foreach (var command in commands)
                {
                    command.Register(package);
                }
            }
        }

        protected abstract Task Execute();

        protected void BeforeQueryStatus(OleMenuCommand command)
        {
            command.Enabled = Enabled;
            command.Visible = Visible;
        }

        protected void Register(Package package, OleMenuCommand command)
        {
            Package = package;
            var serviceProvider = (IServiceProvider)package;
            var mcs = (IMenuCommandService)serviceProvider.GetService(typeof(IMenuCommandService));
            mcs?.AddCommand(command);
        }
    }

    abstract class Command<TParam> : Command
    {
        protected Command(Guid commandSet, int commandId)
            : base(commandSet, commandId)
        {
        }

        public override void Register(Package package)
        {
            var command = new OleMenuCommand(
                (s, e) => Execute((TParam)((OleMenuCmdEventArgs)e).InValue).Forget(),
                (s, e) => { },
                (s, e) => BeforeQueryStatus((OleMenuCommand)s),
                CommandID);
            Register(package, command);
        }

        protected sealed override Task Execute() => Task.CompletedTask;

        protected abstract Task Execute(TParam param);
    }
}
