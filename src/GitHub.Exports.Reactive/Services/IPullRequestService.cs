using GitHub.Api;
using GitHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.Services
{
    public interface IPullRequestService
    {
        IObservable<IPullRequestModel> CreatePullRequest(IRepositoryHost host, ISimpleRepositoryModel repository, string title, IBranch source, IBranch target);
    }
}
