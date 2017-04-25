using System;
using System.ComponentModel.Composition;
using Microsoft.TeamFoundation.Controls;

namespace GitHub.TeamFoundation
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public class ResolvingTeamExplorerSectionAttribute : ExportAttribute
    {
        public string Id { get; }
        public string ParentPageId { get; }
        public int Priority { get; }

        public ResolvingTeamExplorerSectionAttribute(string id, string parentPageId, int priority)
            : base(TeamFoundationResolver.Resolve(() => typeof(ITeamExplorerSection)))
        {
            Id = id;
            ParentPageId = parentPageId;
            Priority = priority;
        }
    }
}
