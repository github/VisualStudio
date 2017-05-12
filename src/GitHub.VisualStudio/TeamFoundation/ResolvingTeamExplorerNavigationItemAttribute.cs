using System;
using System.ComponentModel.Composition;
using Microsoft.TeamFoundation.Controls;

namespace GitHub.VisualStudio.TeamFoundation
{
    [NullGuard.NullGuard(NullGuard.ValidationFlags.None)]
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ResolvingTeamExplorerNavigationItemAttribute : ExportAttribute
    {
        public string Id { get; private set; }
        public int Priority { get; private set; }
        public string TargetPageId { get; set; }

        public ResolvingTeamExplorerNavigationItemAttribute(string id, int priority)
            : base(TeamFoundationResolver.Resolve(() => typeof(ITeamExplorerNavigationItem)))
        {
            Id = id;
            Priority = priority;
        }
    }
}
