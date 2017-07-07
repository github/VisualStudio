using System;
using System.Text;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.InlineReviews.UnitTests.TestDoubles
{
    class FakeEditorContentSource : IEditorContentSource
    {
        byte[] content;

        public FakeEditorContentSource(string content)
        {
            SetContent(content);
        }

        public FakeEditorContentSource(byte[] content)
        {
            SetContent(content);
        }

        public Task<byte[]> GetContent() => Task.FromResult(content);

        public void SetContent(string content)
        {
            this.content = Encoding.UTF8.GetBytes(content);
        }

        public void SetContent(byte[] content)
        {
            this.content = content;
        }
    }
}
