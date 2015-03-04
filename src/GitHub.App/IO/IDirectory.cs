using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;

namespace GitHub.IO
{
    public interface IDirectory : IFileSystemInfo
    {
        IDirectory Parent { get; }
        IDirectory CreateSubdirectory(string path);
        void Delete(bool recursive);
        void Create();
        /// <summary>
        /// Creates the directory and any missing parent directories. Does nothing if the directory exists.
        /// </summary>
        void CreateRecursive();
        bool IsEmpty { get; }
        IEnumerable<IDirectory> EnumerateDirectories();
        IEnumerable<IDirectory> EnumerateDirectories(string searchPattern, SearchOption searchOption);
        IEnumerable<IFile> EnumerateFiles();
        IEnumerable<IFile> EnumerateFiles(string searchPattern);
        IDirectory Root { get; }
        FileAttributes Attributes { get; set; }
        void MoveTo(string destDirName);
        void Refresh();
    }
}
