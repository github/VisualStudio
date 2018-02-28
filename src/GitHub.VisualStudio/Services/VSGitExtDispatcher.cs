using System.ComponentModel.Composition;

namespace GitHub.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSGitExtDispatcher
    {
        readonly IGitHubServiceProvider serviceProvider;

        [ImportingConstructor]
        public VSGitExtDispatcher(IGitHubServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [Export(typeof(IVSGitExt))]
        public IVSGitExt VSGitExt => serviceProvider.GetService<IVSGitExt>();
    }
}
