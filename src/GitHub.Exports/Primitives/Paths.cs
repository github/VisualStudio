using System;
using System.IO;

namespace GitHub.Primitives
{
    /// <summary>
    /// Convert to and from Git paths.
    /// </summary>
    public static class Paths
    {
        public const char GitDirectorySeparatorChar = '/';

        /// <summary>
        /// Convert from a relative path to a Git path.
        /// </summary>
        /// <param name="relativePath">A relative path.</param>
        /// <returns>A working directory relative path which uses the '/' directory separator.</returns>
        public static string ToGitPath(string relativePath)
        {
            return relativePath.Replace(Path.DirectorySeparatorChar, GitDirectorySeparatorChar);
        }

        /// <summary>
        /// Convert from a Git path to a relative path.
        /// </summary>
        /// <param name="gitPath">A working directory relative path which uses the '/' directory separator.</param>
        /// <returns>A relative paht that uses the <see cref="Path.DirectorySeparatorChar"/> directory separator.</returns>
        public static string ToRelativePath(string gitPath)
        {
            return gitPath.Replace(GitDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }
    }
}
