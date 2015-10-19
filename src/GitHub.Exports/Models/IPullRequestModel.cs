using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using GitHub.Collections;

namespace GitHub.Models
{
    public interface IPullRequestModel : ICopyable<IPullRequestModel>,
        IEquatable<IPullRequestModel>, IComparable<IPullRequestModel>
    {
        int Number { get; }
        string Title { get; }
        int CommentCount { get; }
        bool HasNewComments { get; }
        DateTimeOffset CreatedAt { get; }
        DateTimeOffset UpdatedAt { get; }
        IAccount Author { get; }
    }
}
