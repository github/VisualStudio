using System;
using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.StartPage
{
    public class StartPageRepositoryModel : RepositoryModelBase, IRemoteRepositoryModel
    {
        public StartPageRepositoryModel(string name, UriString cloneUrl)
            : base(name, cloneUrl)
        {
        }

        public DateTimeOffset CreatedAt
        {
            get { throw new NotImplementedException(); }
        }

        public IBranch DefaultBranch
        {
            get { throw new NotImplementedException(); }
        }

        public long Id
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsFork
        {
            get { throw new NotImplementedException(); }
        }

        public IAccount OwnerAccount
        {
            get { throw new NotImplementedException(); }
        }

        public IRemoteRepositoryModel Parent
        {
            get { throw new NotImplementedException(); }
        }

        public DateTimeOffset UpdatedAt
        {
            get { throw new NotImplementedException(); }
        }

        public int CompareTo(IRemoteRepositoryModel other)
        {
            throw new NotImplementedException();
        }

        public void CopyFrom(IRemoteRepositoryModel other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(IRemoteRepositoryModel other)
        {
            throw new NotImplementedException();
        }
    }
}
