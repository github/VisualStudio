using System;
using Microsoft.VisualStudio.Shell;

namespace GitHub.InlineReviews.Commands
{
    interface ICommand
    {
        void Register(Package package);
    }
}
