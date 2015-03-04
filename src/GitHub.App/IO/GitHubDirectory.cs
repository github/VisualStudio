using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;

namespace GitHub.IO
{
    public class GitHubDirectory : IDirectory
    {
        readonly DirectoryInfo directory;
        readonly Lazy<IDirectory> parentDirectory;
        readonly Lazy<IDirectory> rootDirectory;

        public GitHubDirectory(string path) : this(new DirectoryInfo(path))
        {
        }

        protected internal GitHubDirectory(DirectoryInfo directory)
        {
            this.directory = directory;
            parentDirectory = new Lazy<IDirectory>(() => directory.Parent == null ? null : new GitHubDirectory(directory.Parent));
            rootDirectory = new Lazy<IDirectory>(() => new GitHubDirectory(directory.Root));
        }

        public string FullName
        {
            get { return directory.FullName; }
        }

        public bool Exists
        {
            get { return directory.Exists; }
        }

        public string Name
        {
            get { return directory.Name; }
        }

        public void Delete(bool recursive)
        {
            directory.Delete(recursive);
        }

        public IDirectory Parent
        {
            get { return parentDirectory.Value; }
        }

        public void Create()
        {
            directory.Create();
        }

        public void CreateRecursive()
        {
            Directory.CreateDirectory(FullName);
        }

        public bool IsEmpty
        {
            get
            {
                if (!Exists) throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "The directory '{0}' does not exist", directory.FullName));
                return !directory.EnumerateDirectories().Any() && !directory.EnumerateFiles().Any();
            }
        }

        public IEnumerable<IDirectory> EnumerateDirectories(string searchPattern, SearchOption searchOption)
        {
            return directory.EnumerateDirectories(searchPattern, searchOption).Select(d => new GitHubDirectory(d));
        }

        public IEnumerable<IDirectory> EnumerateDirectories()
        {
            return directory.EnumerateDirectories().Select(d => new GitHubDirectory(d));
        }

        public IEnumerable<IFile> EnumerateFiles()
        {
            return directory.EnumerateFiles().Select(f => new GitHubFile(f));
        }

        public IEnumerable<IFile> EnumerateFiles(string searchPattern)
        {
            return directory.EnumerateFiles(searchPattern).Select(f => new GitHubFile(f));
        }

        public IDirectory Root
        {
            get { return rootDirectory.Value; }
        }

        public FileAttributes Attributes
        {
            get { return directory.Attributes; }
            set { directory.Attributes = value; }
        }

        public void MoveTo(string destDirName)
        {
            directory.MoveTo(destDirName);
        }

        public void Refresh()
        {
            directory.Refresh();
        }

        public IDirectory CreateSubdirectory(string path)
        {
            return new GitHubDirectory(directory.CreateSubdirectory(path));
        }
    }
}
