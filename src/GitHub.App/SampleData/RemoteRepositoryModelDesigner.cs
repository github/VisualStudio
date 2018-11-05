using System;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.UI;

namespace GitHub.SampleData
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
    public class RemoteRepositoryModelDesigner : RemoteRepositoryModel
    {
        public UriString CloneUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public BranchModel DefaultBranch { get; set; }
        public Octicon Icon { get; set; }
        public long Id { get; set; }
        public bool IsFork { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public IAccount OwnerAccount { get; set; }
        public RemoteRepositoryModel Parent { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public int CompareTo(RemoteRepositoryModel other)
        {
            return 0;
        }

        public void CopyFrom(RemoteRepositoryModel other)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public bool Equals(RemoteRepositoryModel other)
        {
            return false;
        }

        public void SetIcon(bool isPrivate, bool isFork)
        {
        }
    }
}
