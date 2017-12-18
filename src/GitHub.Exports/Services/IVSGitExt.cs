using GitHub.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace GitHub.Services
{
    public interface IVSGitExt
    {
        void Refresh(IServiceProvider serviceProvider);
        IEnumerable<ILocalRepositoryModel> ActiveRepositories { get; }
        event Action ActiveRepositoriesChanged;
    }
}