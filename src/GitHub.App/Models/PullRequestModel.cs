using GitHub.Primitives;
using NullGuard;
using System;
using GitHub.VisualStudio.Helpers;

namespace GitHub.Models
{
    public class PullRequestModel : NotificationAwareObject, IPullRequestModel
    {
        public PullRequestModel(int number, string title, IAccount author, DateTimeOffset createdAt, DateTimeOffset? updatedAt = null)
        {
            Number = number;
            Title = title;
            Author = author;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt ?? CreatedAt;
        }

        public void CopyFrom(IPullRequestModel other)
        {
            Title = other.Title;
            UpdatedAt = other.UpdatedAt;
            CommentCount = other.CommentCount;
            HasNewComments = other.HasNewComments;
        }

        public override bool Equals([AllowNull]object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            var other = obj as PullRequestModel;
            return other != null && Number == other.Number;
        }

        public override int GetHashCode()
        {
            return Number;
        }

        bool IEquatable<IPullRequestModel>.Equals([AllowNull]IPullRequestModel other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return other != null && Number == other.Number;
        }

        public int Number { get; }

        string title;
        public string Title {
            get { return title; }
            set { title = value; this.RaisePropertyChange(); }
        }

        int commentCount;
        public int CommentCount
        {
            get { return commentCount; }
            set { commentCount = value; this.RaisePropertyChange(); }
        }

        bool hasNewComments;
        public bool HasNewComments
        {
            get { return hasNewComments; }
            set { hasNewComments = value; this.RaisePropertyChange(); }
        }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public IAccount Author { get; set; }
    }
}
