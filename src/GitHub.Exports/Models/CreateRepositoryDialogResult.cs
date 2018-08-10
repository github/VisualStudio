using System;
using GitHub.Services;
using Octokit;

namespace GitHub.Models
{
    /// <summary>
    /// Holds the result of a call to <see cref="IDialogService.ShowCreateRepositoryDialog(IConnection)"/>.
    /// </summary>
    public class CreateRepositoryDialogResult
    {
        public CreateRepositoryDialogResult(
            NewRepository newRepository,
            IAccount account,
            string baseRepositoryPath)
        {
            NewRepository = newRepository;
            Account = account;
            BaseRepositoryPath = baseRepositoryPath;
        }

        /// <summary>
        /// Gets the octokit request that will create the repository.
        /// </summary>
        public NewRepository NewRepository { get; }

        /// <summary>
        /// Gets the account that should be used to create the repository.
        /// </summary>
        public IAccount Account { get; }

        /// <summary>
        /// Gets the base path to which the repository will be cloned.
        /// </summary>
        public string BaseRepositoryPath { get; }
    }
}
