using System;

namespace GitHub.Models
{
    public class CompareBufferTag
    {
        public CompareBufferTag(string path, bool isLeftBuffer)
        {
            Path = path;
            IsLeftBuffer = isLeftBuffer;
        }

        public string Path { get; }
        public bool IsLeftBuffer { get; }
    }
}
