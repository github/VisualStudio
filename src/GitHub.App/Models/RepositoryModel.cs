using System;
using GitHub.Primitives;

namespace GitHub.Models
{
    public class RepositoryModel : IRepositoryModel
    {
        public string Description
        {
            get;
            set;
        }

        public HostAddress HostAddress
        {
            get;
            set;
        }

        public Uri HostUri
        {
            get;
            set;
        }

        public int? Id
        {
            get;
            set;
        }

        public bool IsPrivate
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string NameWithOwner
        {
            get;
            set;
        }

        public string Owner
        {
            get;
            set;
        }
    }
}
