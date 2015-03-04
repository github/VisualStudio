using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using GitHub.IO;
using ReactiveUI;

namespace GitHub.Models
{
    public class GitIgnoreTemplate : ReactiveObject, IGitIgnoreTemplate
    {
        readonly bool isRecommended;
        readonly IEnumerable<IFile> additionalFiles;
        readonly IFile file;
        bool isSelected;

        public GitIgnoreTemplate(IFile fileInfo) : this(fileInfo, false, Enumerable.Empty<IFile>()) { }

        public GitIgnoreTemplate(IFile fileInfo, bool recommended) : this(fileInfo, recommended, Enumerable.Empty<IFile>()) { }

        public GitIgnoreTemplate(IFile fileInfo, bool recommended, IEnumerable<IFile> ignoresToAppend)
        {
            isRecommended = recommended;
            file = fileInfo;
            additionalFiles = ignoresToAppend;
        }

        public string Name
        {
            get { return Path.GetFileNameWithoutExtension(file.Name); }
        }

        public bool CanCopy { get { return file.Exists; } }

        public void CopyTo(string target, bool overwrite)
        {
            if (CanCopy)
            {
                file.CopyTo(target, overwrite);
            }

            var targetFile = new GitHubFile(target);
            if (targetFile.Exists)
            {
                AppendAnyExtraIgnoreLines(targetFile);
            }
        }

        void AppendAnyExtraIgnoreLines(IFile targetFile)
        {
            var sectionLine = string.Format(CultureInfo.InvariantCulture, "{0}# ========================={0}", Environment.NewLine);

            targetFile.AppendText(sectionLine);
            targetFile.AppendText("# Operating System Files");
            targetFile.AppendText(sectionLine);

            foreach (var additionalFile in additionalFiles)
            {
                targetFile.AppendText(string.Format(CultureInfo.InvariantCulture, "{0}# {1}", Environment.NewLine, additionalFile.NameWithoutExtension));
                targetFile.AppendText(sectionLine);
                targetFile.AppendText(Environment.NewLine);
                targetFile.AppendText(additionalFile.ReadAllText(Encoding.UTF8));
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