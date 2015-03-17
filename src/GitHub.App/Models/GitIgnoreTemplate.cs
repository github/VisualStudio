using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using ReactiveUI;
using Rothko;

namespace GitHub.Models
{
    public class GitIgnoreTemplate : ReactiveObject, IGitIgnoreTemplate
    {
        readonly bool isRecommended;
        readonly IEnumerable<IFileInfo> additionalFiles;
        readonly IFileInfo file;
        bool isSelected;

        public GitIgnoreTemplate(IFileInfo file) : this(file, false, Enumerable.Empty<IFileInfo>()) { }

        public GitIgnoreTemplate(IFileInfo file, bool recommended) : this(file, recommended, Enumerable.Empty<IFileInfo>()) { }

        public GitIgnoreTemplate(IFileInfo file, bool recommended, IEnumerable<IFileInfo> ignoresToAppend)
        {
            isRecommended = recommended;
            this.file = file;
            additionalFiles = ignoresToAppend;
        }

        public string Name
        {
            get { return Path.GetFileNameWithoutExtension(file.Name); }
        }

        public bool CanCopy { get { return file.Exists; } }

        public async Task CopyTo(string target, bool overwrite)
        {
            if (CanCopy)
            {
                file.CopyTo(target, overwrite);
            }

            var targetFile = new Rothko.FileInfo(target); // TODO: We shouln't create this direcly. :(
            if (targetFile.Exists)
            {
                await AppendAnyExtraIgnoreLines(targetFile);
            }
        }

        async Task AppendAnyExtraIgnoreLines(IFileInfo targetFile)
        {
            var sectionLine = string.Format(CultureInfo.InvariantCulture, "{0}# ========================={0}", System.Environment.NewLine);

            await targetFile.AppendText(sectionLine);
            await targetFile.AppendText("# Operating System Files");
            await targetFile.AppendText(sectionLine);

            foreach (var additionalFile in additionalFiles)
            {
                await targetFile.AppendText(string.Format(CultureInfo.InvariantCulture, "{0}# {1}", System.Environment.NewLine, Path.GetFileNameWithoutExtension(additionalFile.Name)));
                await targetFile.AppendText(sectionLine);
                await targetFile.AppendText(System.Environment.NewLine);
                await targetFile.AppendText(await additionalFile.ReadAllText());
            }
        }

        public string FilePath { get { return file.FullName; } }

        public bool IsSelected
        {
            get { return isSelected; }
            set { this.RaiseAndSetIfChanged(ref isSelected, value); }
        }

        public bool IsRecommended { get { return isRecommended; } }
    }
}