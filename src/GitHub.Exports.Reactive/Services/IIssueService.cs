using System;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IIssueService
    {
        Task<Page<IssueListModel>> GetIssues(
            IRepositoryModel repository,
            string after,
            bool refresh);
    }
}