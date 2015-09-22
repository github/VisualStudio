using System;
using System.Windows.Media.Imaging;

namespace GitHub.Models
{
    public interface IPullRequestModel
    {
        int Number { get; }
        string Title { get; }
        int CommentCount { get; }
        bool HasNewComments { get; }
        DateTimeOffset CreatedAt { get; }
        IAccount Author { get; }
    }
}
