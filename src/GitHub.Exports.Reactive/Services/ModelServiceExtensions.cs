using System;
using GitHub.Models;

namespace GitHub.Services
{
    public static class ModelServiceExtensions
    {
        static ModelServiceExtensions()
        {
            System.Diagnostics.Debugger.Break();
        }


        public static IObservable<IPullRequestModel> GetPullRequest(this IModelService service, IRepositoryModel repo, int number)
        {
            return service.GetPullRequest(repo.Owner, repo.Name, number);
        }
    }
}
