using System;
using System.Threading;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IIssueService
    {
        IObservable<Page<IssueListModel>> GetIssues(
            IRepositoryModel repository,
            CancellationToken cancel);
    }
}