using System;
using System.ComponentModel.Composition;
using Microsoft.TeamFoundation.Controls;

namespace GitHub.TeamFoundation
{
    //[MetadataAttribute]
    public class ResolvingTeamExplorerNavigationItemAttribute : ExportAttribute
    {
        //public string Id { get; }
        //public int Priority { get; }
        //public string TargetPageId { get; set; }

        public ResolvingTeamExplorerNavigationItemAttribute(/*string id, int priority*/)
            : base(TeamFoundationResolver.Resolve(() => typeof(ITeamExplorerNavigationItem)))
        {
            //Id = id;
            //Priority = priority;
        }
    }
}
