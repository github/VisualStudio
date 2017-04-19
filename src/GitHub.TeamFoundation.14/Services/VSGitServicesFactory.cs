using System;
using System.ComponentModel.Composition;
using GitHub.TeamFoundation;

namespace GitHub.Services
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSGitServicesFactory
    {
        readonly IGitHubServiceProvider serviceProvider;

        [ImportingConstructor]
        public VSGitServicesFactory(IGitHubServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [Export(typeof(IVSGitServices))]
        public IVSGitServices VSGitServices =>
            (IVSGitServices)TeamFoundationResolver.Resolve(() => new VSGitServices(serviceProvider));
    }
}
