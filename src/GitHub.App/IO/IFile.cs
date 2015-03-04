using System;
using System.IO;
using System.Text;

namespace GitHub.IO
{
    public interface IFile : IFileSystemInfo
    {
        IDirectory Directory { get; }

        string NameWithoutExtension { get; }

        void Delete();

        void MoveTo(string target);

        void CopyTo(string target, bool overwrite);

        void WriteAllText(string contents, Encoding encoding);

        void WriteAllBytes(byte[] bytes);

        string ReadAllText(Encoding encoding);

        byte[] ReadAllBytes();

        long Length { get; }

        void Refresh();

        DateTime GetLastWriteTimeUtc();

        void AppendText(string text);

        FileStream OpenRead();
    }
}
