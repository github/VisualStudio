using System;
using System.ComponentModel;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.UI;
using GitHub.Exports;

namespace GitHub.App.SampleData
{
    public class LocalRepositoryModelDesigner : ILocalRepositoryModel
    {
        public UriString CloneUrl { get; set; }
        public IBranch CurrentBranch { get; set; }
        public Octicon Icon { get; set; }
        public string LocalPath { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }

#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

        public Task<UriString> GenerateUrl(LinkType linkType, string path = null, int startLine = -1, int endLine = -1)
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public void SetIcon(bool isPrivate, bool isFork)
        {
            throw new NotImplementedException();
        }
    }
}
