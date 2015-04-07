using System;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using LibGit2Sharp;

namespace GitHub.Services
{
    [Export(typeof(IGitClient))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GitClient : IGitClient
    {
        public IObservable<Unit> Push(IRepository repository, string branchName, string remoteName)
        {
            Guard.ArgumentNotEmptyString(branchName, "branchName");
            Guard.ArgumentNotEmptyString(remoteName, "remoteName");

            return Observable.Defer(() =>
            {
                // TODO: Need to handle error conditions.
                var remote = repository.Network.Remotes[remoteName];
                repository.Network.Push(remote, "HEAD", @"refs/heads/" + branchName);
                return Observable.Return(Unit.Default);
            });
        }

        public IObservable<Unit> SetRemote(IRepository repository, string remoteName, Uri url)
        {
            Guard.ArgumentNotEmptyString(remoteName, "remoteName");

            return Observable.Defer(() =>
            {
                repository.Config.Set("remote." + remoteName + ".url", url.ToString());
                repository.Config.Set("remote." + remoteName + ".fetch", "+refs/heads/*:refs/remotes/" + remoteName + "/*");

                return Observable.Return(Unit.Default);
            });
        }
    }
}
