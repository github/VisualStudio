using ReactiveUI;
using System;

namespace GitHub.Extensions
{
    public class RecoveryCommandWithIcon : RecoveryCommand
    {
        public string Icon { get; private set; }

        public RecoveryCommandWithIcon(string commandName, string icon, Func<object, RecoveryOptionResult> handler = null) : base(commandName, handler)
        {
            Icon = icon;
        }
    }
}
