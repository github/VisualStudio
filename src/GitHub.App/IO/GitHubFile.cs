using System;
using System.IO;
using System.Text;
using GitHub.Helpers;

namespace GitHub.IO
{
    public class GitHubFile : IFile
    {
        readonly FileInfo file;
        readonly Lazy<IDirectory> directory;

        public GitHubFile(string path) : this(new FileInfo(path))
        {
        }

        protected internal GitHubFile(FileInfo file)
        {
            this.file = file;
            directory = new Lazy<IDirectory>(() => new GitHubDirectory(file.Directory));
        }

        public string Name
        {
            get { return file.Name; }
        }

        public string NameWithoutExtension
        {
            get { return Path.GetFileNameWithoutExtension(file.Name); }
        }

        public string FullName
        {
            get { return file.FullName; }
        }

        public bool Exists
        {
            get { return file.Exists; }
        }

        public long Length
        {
            get { return file.Length; }
        }

        public IDirectory Directory
        {
            get { return directory.Value; }
        }

        public void Delete()
        {
            file.Delete();
        }

        public void MoveTo(string target)
        {
            File.Move(FullName, target);
        }

        public void CopyTo(string target, bool overwrite)
        {
            File.Copy(FullName, target, overwrite);
        }

        public void WriteAllText(string contents, Encoding encoding)
        {
            File.WriteAllText(FullName, contents, encoding);
        }

        public void WriteAllBytes(byte[] bytes)
        {
            File.WriteAllBytes(FullName, bytes);
        }

        public string ReadAllText(Encoding encoding)
        {
            return File.ReadAllText(FullName, encoding);
        }

        public byte[] ReadAllBytes()
        {
            return File.ReadAllBytes(FullName);
        }

        public void Refresh()
        {
            file.Refresh();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override string ToString()
        {
            return FullName;
        }

        public DateTime GetLastWriteTimeUtc()
        {
            return file.LastWriteTimeUtc;
        }

        public void AppendText(string text)
        {
            using (var writer = file.AppendText())
            {
                writer.Write(text);
            }
        }

        public FileStream OpenRead()
        {
            return file.OpenRead();
        }
    }
}
