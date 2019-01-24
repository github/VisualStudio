using System;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using GitHub.Models;
using GitHub.Validation;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Base class for the Repository publish/create dialogs. It represents the details about the repository itself.
    /// </summary>
    public abstract class RepositoryFormViewModel : ViewModelBase
    {
        readonly ObservableAsPropertyHelper<string> safeRepositoryName;

        protected RepositoryFormViewModel()
        {
            safeRepositoryName = this.WhenAny(x => x.RepositoryName, x => x.Value)
                .Select(x => x != null ? GetSafeRepositoryName(x) : null)
                .ToProperty(this, x => x.SafeRepositoryName);
        }

        string description;
        /// <summary>
        /// Description to set on the repo (optional)
        /// </summary>
        public string Description
        {
            get { return description; }
            set { this.RaiseAndSetIfChanged(ref description, value); }
        }

        bool keepPrivate;
        /// <summary>
        /// Make the new repository private
        /// </summary>
        public bool KeepPrivate
        {
            get { return keepPrivate; }
            set { this.RaiseAndSetIfChanged(ref keepPrivate, value); }
        }

        string repositoryName;
        /// <summary>
        /// Name of the repository as typed by user
        /// </summary>
        public string RepositoryName
        {
            get { return repositoryName; }
            set { this.RaiseAndSetIfChanged(ref repositoryName, value); }
        }

        public ReactivePropertyValidator<string> RepositoryNameValidator { get; protected set; }

        /// <summary>
        /// Name of the repository after fixing it to be safe (dashes instead of spaces, etc)
        /// </summary>
        public string SafeRepositoryName
        {
            get { return safeRepositoryName.Value; }
        }

        public ReactivePropertyValidator<string> SafeRepositoryNameWarningValidator { get; protected set; }

        IAccount selectedAccount;
        /// <summary>
        /// Account where the repository is going to be created on
        /// </summary>
        public IAccount SelectedAccount
        {
            get { return selectedAccount; }
            set { this.RaiseAndSetIfChanged(ref selectedAccount, value); }
        }

        // These are the characters which are permitted when creating a repository name on GitHub The Website
        static readonly Regex invalidRepositoryCharsRegex = new Regex(@"[^0-9A-Za-z_\.\-]", RegexOptions.ECMAScript);

        /// <summary>
        /// Given a repository name, returns a safe version with invalid characters replaced with dashes.
        /// </summary>
        protected static string GetSafeRepositoryName(string name)
        {
            return invalidRepositoryCharsRegex.Replace(name, "-");
        }

        protected virtual Octokit.NewRepository GatherRepositoryInfo()
        {
            return new Octokit.NewRepository(RepositoryName)
            {
                Description = Description,
                Private = KeepPrivate
            };
        }
    }
}
