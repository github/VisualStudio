using System.Threading.Tasks;
using Rothko;

namespace GitHub.Extensions
{
    public static class FileExtensions
    {
        public static async Task AppendText(this IFileInfo fileInfo, string text)
        {
            using (var writer = fileInfo.AppendText())
            {
                await writer.WriteAsync(text);
            }
        }

        public static async Task<string> ReadAllText(this IFileInfo fileInfo)
        {
            using (var reader = fileInfo.OpenText())
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
