using System;
using System.ComponentModel.Composition;
using GitHub.Services;

namespace GitHub.VisualStudio.TeamFoundation
{
    public class VSGitServicesFactory
    {
        [ImportingConstructor]
        public VSGitServicesFactory([TeamFoundationResolver] IResolver resolver,
            IGitHubServiceProvider serviceProvider)
        {
            VSGitServices = new VSGitServices(serviceProvider);
        }

        [Export(typeof(IVSGitServices))]
        public IVSGitServices VSGitServices { get; }
    }
}
