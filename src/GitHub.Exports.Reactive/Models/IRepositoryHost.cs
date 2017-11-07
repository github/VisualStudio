using System;
using System.Reactive;
using GitHub.Api;
using GitHub.Authentication;
using GitHub.Primitives;
using GitHub.Services;

namespace GitHub.Models
{
    public interface IRepositoryHost
    {
        HostAddress Address { get; }
        IApiClient ApiClient { get; }
        IModelService ModelService { get; }
        bool IsLoggedIn { get; }
        string Title { get; }
    }
}
