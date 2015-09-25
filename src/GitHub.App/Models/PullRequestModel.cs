using GitHub.Services;
using NullGuard;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media.Imaging;

namespace GitHub.Models
{
    public class PullRequestModel : IPullRequestModel
    {
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

        public int Number { get; set; }
        public string Title { get; set; }
        public int CommentCount { get; set; }
        public bool HasNewComments { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public IAccount Author { get; set; }
    }
}
