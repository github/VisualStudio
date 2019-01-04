using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using GitHub.Caches;
using GitHub.Extensions;
using GitHub.Helpers;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels;

namespace GitHub.Services
{
    /// <summary>
    /// Supplies @mentions auto complete suggestions.
    /// </summary>
    [Export(typeof(IAutoCompleteSource))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MentionsAutoCompleteSource : IAutoCompleteSource
    {
        readonly Lazy<IMentionsCache> mentionsCache;
        readonly Lazy<ISourceListViewModel> currentRepositoryState;
        readonly Lazy<IImageCache> imageCache;
        readonly IAvatarProvider hostAvatarProvider;

        [ImportingConstructor]
        public MentionsAutoCompleteSource(
            Lazy<IMentionsCache> mentionsCache,
            Lazy<IImageCache> imageCache,
            IAvatarProvider hostAvatarProvider)
        {
            Guard.ArgumentNotNull(mentionsCache, "mentionsCache");
            Guard.ArgumentNotNull(currentRepositoryState, "currentRepositoryState");
            Guard.ArgumentNotNull(imageCache, "imageCache");
            Guard.ArgumentNotNull(hostAvatarProvider, "hostAvatarProvider");

            this.mentionsCache = mentionsCache;
            this.currentRepositoryState = currentRepositoryState;
            this.imageCache = imageCache;
            this.hostAvatarProvider = hostAvatarProvider;
        }

        public IObservable<AutoCompleteSuggestion> GetSuggestions()
        {
            if (CurrentRepository.RepositoryHost == null)
            {
                return Observable.Empty<AutoCompleteSuggestion>();
            }

            var avatarProviderKey = CurrentRepository.RepositoryHost.Address.WebUri.ToString();
            var avatarProvider = hostAvatarProvider.Get(avatarProviderKey);

            Func<Uri, IObservable<BitmapSource>> resolveImage = uri =>
                Observable.Defer(() => ImageCache
                    .GetImage(uri)
                    .Catch<BitmapSource, Exception>(_ => Observable.Return(avatarProvider.DefaultUserBitmapImage))
                    .StartWith(avatarProvider.DefaultUserBitmapImage));

            return MentionsCache.RetrieveSuggestions(CurrentRepository)
                .Catch<IReadOnlyList<SuggestionItem>, Exception>(_ => Observable.Empty<IReadOnlyList<SuggestionItem>>())
                .SelectMany(x => x.ToObservable())
                .Where(suggestion => !String.IsNullOrEmpty(suggestion.Name)) // Just being extra cautious
                .Select(suggestion =>
                    new AutoCompleteSuggestion(suggestion.Name, suggestion.Description, resolveImage(suggestion.IconKey), Prefix));
        }

        public string Prefix { get { return "@"; } }

        IImageCache ImageCache { get { return imageCache.Value; } }

        IMentionsCache MentionsCache { get { return mentionsCache.Value; } }

        RepositoryModel CurrentRepository { get { return currentRepositoryState.Value.SelectedRepository; } }
    }
}
