using System;
using System.ComponentModel.Design;
using GitHub.Commands;

namespace GitHub.Services.Vssdk.Commands
{
    public static class MenuCommandServiceExtensions
    {
        public static void AddCommand(this IMenuCommandService service, IVsCommandBase command)
        {
            service.AddCommand((MenuCommand)command);
        }
    }
}
