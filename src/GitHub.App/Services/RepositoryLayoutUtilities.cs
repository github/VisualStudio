using System;
using System.IO;
using GitHub.Primitives;

namespace GitHub.Services
{
    public static class RepositoryLayoutUtilities
    {
        public static string GetDefaultRepositoryPath(UriString cloneUrl, string defaultPath, RepositoryLayout repositoryLayout)
        {
            switch (repositoryLayout)
            {
                case RepositoryLayout.Name:
                    return Path.Combine(defaultPath, cloneUrl.RepositoryName);
                case RepositoryLayout.OwnerName:
                    return Path.Combine(defaultPath, cloneUrl.Owner, cloneUrl.RepositoryName);
                default:
                    throw new ArgumentException($"Unknown repository layout: {repositoryLayout}");

            }
        }

        public static RepositoryLayout GetRepositoryLayout(string repositoryLayoutSetting)
        {
            RepositoryLayout repositoryLayout;
            if (!Enum.TryParse(repositoryLayoutSetting, out repositoryLayout))
            {
                repositoryLayout = RepositoryLayout.OwnerName;
            }

            return repositoryLayout;
        }

        public static (string, RepositoryLayout) GetDefaultPathAndLayout(string repositoryPath, UriString cloneUrl)
        {
            var possibleOwnerPath = Path.GetDirectoryName(repositoryPath);
            var possibleOwner = Path.GetFileName(possibleOwnerPath);
            if (string.Equals(possibleOwner, cloneUrl.Owner, StringComparison.OrdinalIgnoreCase))
            {
                return (Path.GetDirectoryName(possibleOwnerPath), RepositoryLayout.OwnerName);
            }
            else
            {
                return (possibleOwnerPath, RepositoryLayout.Name);
            }
        }
    }
}
