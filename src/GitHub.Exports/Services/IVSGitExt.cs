using System;
using System.Collections.Generic;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IVSGitExt
    {
        IEnumerable<ILocalRepositoryModel> ActiveRepositories { get; }
        event Action ActiveRepositoriesChanged;
    }
}