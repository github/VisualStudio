using System;
using System.Globalization;
using GitHub.Primitives;
using GitHub.VisualStudio.Helpers;
using System.Diagnostics;
using System.Collections.Generic;
using GitHub.Extensions;

namespace GitHub.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class PullRequestModel : NotificationAwareObject, IPullRequestModel,
        IEquatable<PullRequestModel>,
        IComparable<PullRequestModel>
    {
        public PullRequestModel(int number, string title, IAccount author,
            DateTimeOffset createdAt, DateTimeOffset? updatedAt = null)
        {
            Number = number;
            Title = title;
            Author = author;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt ?? CreatedAt;
        }

        public void CopyFrom(IPullRequestModel other)
        {
            if (!Equals(other))
                throw new ArgumentException("Instance to copy from doesn't match this instance. this:(" + this + ") other:(" + other + ")", nameof(other));
            Title = other.Title;
            State = other.State;
            UpdatedAt = other.UpdatedAt;
            CommentCount = other.CommentCount;
            HasNewComments = other.HasNewComments;
            Assignee = other.Assignee;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            var other = obj as PullRequestModel;
            return other != null && Number == other.Number;
        }

        public override int GetHashCode()
        {
            return Number.GetHashCode();
        }

        bool IEquatable<IPullRequestModel>.Equals(IPullRequestModel other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return other != null && Number == other.Number;
        }

        bool IEquatable<PullRequestModel>.Equals(PullRequestModel other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return other != null && Number == other.Number;
        }

        public int CompareTo(IPullRequestModel other)
        {
            return other != null ? UpdatedAt.CompareTo(other.UpdatedAt) : 1;
        }

        public int CompareTo(PullRequestModel other)
        {
            return other != null ? UpdatedAt.CompareTo(other.UpdatedAt) : 1;
        }

        public static bool operator >(PullRequestModel lhs, PullRequestModel rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return false;
            return lhs?.CompareTo(rhs) > 0;
        }

        public static bool operator <(PullRequestModel lhs, PullRequestModel rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return false;
            return (object)lhs == null || lhs.CompareTo(rhs) < 0;
        }

        public static bool operator ==(PullRequestModel lhs, PullRequestModel rhs)
        {
            return ReferenceEquals(lhs, rhs);
        }

        public static bool operator !=(PullRequestModel lhs, PullRequestModel rhs)
        {
            return !(lhs == rhs);
        }

        public int Number { get; }

        string title;
        public string Title
        {
            get { return title; }
            set
            {
                Guard.ArgumentNotNull(value, nameof(value));
                title = value;
                this.RaisePropertyChange();
            }
        }

        PullRequestState status;
        public PullRequestState State
        {
            get { return status; }
            set
            {
                status = value;
                this.RaisePropertyChange();

                // TODO: These notifications will be removed once maintainer workflow has been merged to master.
                this.RaisePropertyChange(nameof(IsOpen));
                this.RaisePropertyChange(nameof(Merged));
            }
        }

        // TODO: Remove these property once maintainer workflow has been merged to master.
        public bool IsOpen => State == PullRequestState.Open;
        public bool Merged => State == PullRequestState.Merged;

        int commentCount;
        public int CommentCount
        {
            get { return commentCount; }
            set { commentCount = value; this.RaisePropertyChange(); }
        }

        int commitCount;
        public int CommitCount
        {
            get { return commitCount; }
            set { commitCount = value; this.RaisePropertyChange(); }
        }

        bool hasNewComments;
        public bool HasNewComments
        {
            get { return hasNewComments; }
            set { hasNewComments = value; this.RaisePropertyChange(); }
        }

        string body;
        public string Body
        {
            get { return body; }
            set { body = value; this.RaisePropertyChange(); }
        }

        public GitReferenceModel Base { get; set; }
        public GitReferenceModel Head { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public IAccount Author { get; set; }

        IAccount assignee;
        public IAccount Assignee
        {
            get { return assignee; }
            set { assignee = value; this.RaisePropertyChange(); }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "id:{0} title:{1} created:{2:u} updated:{3:u}", Number, Title, CreatedAt, UpdatedAt);
        }

        internal string DebuggerDisplay
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "id:{0} title:{1} created:{2:u} updated:{3:u}", Number, Title, CreatedAt, UpdatedAt);
            }
        }
    }
}
