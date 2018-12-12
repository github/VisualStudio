using System;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.UI;

namespace GitHub.SampleData
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
    public class RemoteRepositoryModelDesigner : RemoteRepositoryModel
    {
        public new UriString CloneUrl { get; set; }
        public new DateTimeOffset CreatedAt { get; set; }
        public new BranchModel DefaultBranch { get; set; }
        public new Octicon Icon { get; set; }
        public new long Id { get; set; }
        public new bool IsFork { get; set; }
        public new string Name { get; set; }
        public new string Owner { get; set; }
        public new IAccount OwnerAccount { get; set; }
        public new RemoteRepositoryModel Parent { get; set; }
        public new DateTimeOffset UpdatedAt { get; set; }
    }
}
