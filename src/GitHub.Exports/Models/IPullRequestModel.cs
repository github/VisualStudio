using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using GitHub.Helpers;

namespace GitHub.Models
{
    public interface IPullRequestModel : ICopyable<IPullRequestModel>, IEquatable<IPullRequestModel>
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
