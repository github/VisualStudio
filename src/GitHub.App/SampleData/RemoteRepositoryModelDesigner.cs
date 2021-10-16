using System;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.UI;

namespace GitHub.SampleData
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
    public class RemoteRepositoryModelDesigner : IRemoteRepositoryModel
    {
        public UriString CloneUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public IBranch DefaultBranch { get; set; }
        public Octicon Icon { get; set; }
        public long Id { get; set; }
        public bool IsFork { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public IAccount OwnerAccount { get; set; }
        public IRemoteRepositoryModel Parent { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public int CompareTo(IRemoteRepositoryModel other)
        {
            return 0;
        }

        public void CopyFrom(IRemoteRepositoryModel other)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public bool Equals(IRemoteRepositoryModel other)
        {
            return false;
        }

        public void SetIcon(bool isPrivate, bool isFork)
        {
        }
    }
}
