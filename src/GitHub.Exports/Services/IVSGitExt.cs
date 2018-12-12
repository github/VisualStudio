using System;
using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IVSGitExt
    {
        IReadOnlyList<LocalRepositoryModel> ActiveRepositories { get; }
        event Action ActiveRepositoriesChanged;
        void RefreshActiveRepositories();
    }
}