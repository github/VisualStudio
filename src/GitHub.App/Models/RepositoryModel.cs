using System;
using GitHub.Primitives;
using NullGuard;
using ReactiveUI;
using Octokit;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Globalization;

namespace GitHub.Models
{
    public class RepositoryModel : ReactiveObject, IRepositoryModel
    {
        int? id;
        string name;
        string description;
        bool isPrivate = true; // All repos are assumed to be private until proven otherwise.
        readonly ObservableAsPropertyHelper<string> nameWithOwner;
        string owner;
        Uri hostUri;
        UriString cloneUrl;

        [AllowNull]
        public string Description
        {
            [return: AllowNull]
            get { return description; }
            set { this.RaiseAndSetIfChanged(ref description, value); }
        }

        [AllowNull]
        public HostAddress HostAddress
        {
            [return: AllowNull]
            get;
            set;
        }

        [AllowNull]
        public Uri HostUri
        {
            [return: AllowNull]
            get { return hostUri; }
            set { this.RaiseAndSetIfChanged(ref hostUri, value); }
        }

        public int? Id
        {
            get { return id; }
            set { this.RaiseAndSetIfChanged(ref id, value); }
        }

        public bool IsPrivate
        {
            get { return isPrivate; }
            set { this.RaiseAndSetIfChanged(ref isPrivate, value); }
        }

        public string Name
        {
            get { return name; }
            set { this.RaiseAndSetIfChanged(ref name, value); }
        }

        public string NameWithOwner
        {
            get { return nameWithOwner.Value; }
        }

        [AllowNull]
        public string Owner
        {
            [return: AllowNull]
            get { return owner; }
            set { this.RaiseAndSetIfChanged(ref owner, value); }
        }

        [AllowNull]
        public UriString CloneUrl
        {
            [return: AllowNull]
            get { return cloneUrl; }
            set { this.RaiseAndSetIfChanged(ref cloneUrl, value); }
        }

        public RepositoryModel()
        {
            nameWithOwner = this.WhenAny(x => x.Name, x => x.Owner, (name, owner) => new { Name = name.Value, Owner = owner.Value })
                .Select(x => x.Name != null && x.Owner != null ? String.Format(CultureInfo.InvariantCulture, "{0}/{1}", x.Owner, x.Name) : null)
                .ToProperty(this, x => x.NameWithOwner, null, Scheduler.Immediate);
        }

        public RepositoryModel(Repository repo)
        {
            Id = repo.Id;
            Name = repo.Name;
            Description = repo.Description;
            Owner = repo.Owner.Login;
            CloneUrl = repo.CloneUrl;
            IsPrivate = repo.Private;
        }
    }
}
