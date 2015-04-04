using System;
using GitHub.Primitives;
using GitHub.UI;

namespace GitHub.Models
{
    public interface IRepositoryModel
    {
        HostAddress HostAddress { get; }
        int? Id { get; }
        IAccount Owner { get; set; }
        string Name { get; }
        string NameWithOwner { get; }
        string Description { get; }
        Uri HostUri { get; }
        bool IsPrivate { get; }
        UriString CloneUrl { get; }
        bool HasLocalClone { get; }
        Octicon Icon { get; }
    }
}
