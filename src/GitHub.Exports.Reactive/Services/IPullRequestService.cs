using GitHub.Api;
using GitHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.ViewModels;
using System.Reactive;

namespace GitHub.Services
{
    public interface IPullRequestService
    {
        IObservable<IPullRequestModel> CreatePullRequest(IRepositoryHost host, ISimpleRepositoryModel repository, string title, string body, IBranch source, IBranch target);

        void Checkout(ISimpleRepositoryModel repository, IPullRequestModel pullRequest, string localBranch);
    }
}
