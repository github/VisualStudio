using System;
using System.IO;

namespace GitHub.Primitives
{
    public static class Paths
    {
        public const char GitDirectorySeparatorChar = '/';

        public static string ToGitPath(string relativePath)
        {
            return relativePath.Replace(Path.DirectorySeparatorChar, GitDirectorySeparatorChar);
        }

        public static string ToRelativePath(string relativePath)
        {
            return relativePath.Replace(GitDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }
    }
}
