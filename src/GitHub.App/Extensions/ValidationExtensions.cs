using GitHub.Validation;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.Extensions
{
    public static class ValidationExtensions
    {
        public static ReactivePropertyValidator<string> CreateBaseRepositoryPathValidator(
            this IRepositoryCreationTarget target)
        {
             return ReactivePropertyValidator.ForObservable(target.WhenAny(x => x.BaseRepositoryPath, x => x.Value))
                .IfNullOrEmpty("Please enter a repository path")
                .IfTrue(x => x.Length > 200, "Path too long")
                .IfContainsInvalidPathChars("Path contains invalid characters")
                .IfPathNotRooted("Please enter a valid path");
        }
    }
}
