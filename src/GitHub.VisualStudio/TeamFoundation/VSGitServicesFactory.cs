using System;
using System.ComponentModel.Composition;
using GitHub.TeamFoundation;

namespace GitHub.Services
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSGitServicesFactory
    {
        readonly Lazy<IVSGitServices> vsGitServices;

        [ImportingConstructor]
        public VSGitServicesFactory(IGitHubServiceProvider serviceProvider)
        {
            vsGitServices = new Lazy<IVSGitServices>(() =>
                (IVSGitServices)TeamFoundationResolver.Resolve(() => new VSGitServices(serviceProvider)));
        }

        [Export(typeof(IVSGitServices))]
        public IVSGitServices VSGitServices => vsGitServices.Value;
    }
}
