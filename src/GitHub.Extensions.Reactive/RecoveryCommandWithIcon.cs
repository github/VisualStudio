using System;
using ReactiveUI;
using ReactiveUI.Legacy;

namespace GitHub.Extensions
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class RecoveryCommandWithIcon : RecoveryCommand
    {
        public string Icon { get; private set; }

        public RecoveryCommandWithIcon(string commandName, string icon, Func<object, RecoveryOptionResult> handler = null) : base(commandName, handler)
        {
            Icon = icon;
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
