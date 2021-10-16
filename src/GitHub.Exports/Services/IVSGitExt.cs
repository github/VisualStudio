using System;
using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IVSGitExt
    {
        IReadOnlyList<ILocalRepositoryModel> ActiveRepositories { get; }
        event Action ActiveRepositoriesChanged;
        void RefreshActiveRepositories();
    }
}