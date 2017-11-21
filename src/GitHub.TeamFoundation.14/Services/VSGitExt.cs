using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;

namespace GitHub.VisualStudio.Base
{
    [Export(typeof(IVSGitExt))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VSGitExt : IVSGitExt
    {
        IGitExt gitService;

        public void Refresh(IServiceProvider serviceProvider)
        {
            if (gitService != null)
                gitService.PropertyChanged -= CheckAndUpdate;
            gitService = serviceProvider.GetServiceSafe<IGitExt>();
            if (gitService != null)
                gitService.PropertyChanged += CheckAndUpdate;
        }


        void CheckAndUpdate(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Guard.ArgumentNotNull(e, nameof(e));
            if (e.PropertyName != "ActiveRepositories" || gitService == null)
                return;
            ActiveRepositoriesChanged?.Invoke();
        }

        public IEnumerable<ILocalRepositoryModel> ActiveRepositories => gitService?.ActiveRepositories.Select(x => x.ToModel());
        public event Action ActiveRepositoriesChanged;
    }
}