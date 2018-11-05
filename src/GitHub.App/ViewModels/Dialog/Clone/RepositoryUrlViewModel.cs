using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.ViewModels;
using GitHub.ViewModels.Dialog.Clone;
using ReactiveUI;

namespace GitHub.App.ViewModels.Dialog.Clone
{
    [Export(typeof(IRepositoryUrlViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryUrlViewModel : ViewModelBase, IRepositoryUrlViewModel
    {
        ObservableAsPropertyHelper<RepositoryModel> repository;
        string url;

        public RepositoryUrlViewModel()
        {
            repository = this.WhenAnyValue(x => x.Url, ParseUrl).ToProperty(this, x => x.Repository);
        }

        public string Url
        {
            get => url;
            set => this.RaiseAndSetIfChanged(ref url, value);
        }

        public bool IsEnabled => true;

        public RepositoryModel Repository => repository.Value;

        public Task Activate() => Task.CompletedTask;

        RepositoryModel ParseUrl(string s)
        {
            if (s != null)
            {
                var uri = new UriString(s);

                if (string.IsNullOrWhiteSpace(uri.Owner) || !string.IsNullOrWhiteSpace(uri.RepositoryName))
                {
                    var parts = s.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length == 2)
                    {
                        uri = UriString.ToUriString(
                            HostAddress.GitHubDotComHostAddress.WebUri
                                .Append(parts[0])
                                .Append(parts[1]));
                    }
                }

                if (!string.IsNullOrWhiteSpace(uri.Owner) && !string.IsNullOrWhiteSpace(uri.RepositoryName))
                {
                    return new RepositoryModel(uri.RepositoryName, uri);
                }
            }

            return null;
        }
    }
}
