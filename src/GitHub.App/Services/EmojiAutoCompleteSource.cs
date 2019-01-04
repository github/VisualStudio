using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using GitHub.UI;

namespace GitHub.Services
{
    [Export(typeof(IAutoCompleteSource))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class EmojiAutoCompleteSource : IAutoCompleteSource
    {
        readonly IEmojiCache emojiCache;

        [ImportingConstructor]
        public EmojiAutoCompleteSource(IEmojiCache emojiCache)
        {
            Ensure.ArgumentNotNull(emojiCache, "emojiCache");

            this.emojiCache = emojiCache;
        }

        public IObservable<AutoCompleteSuggestion> GetSuggestions()
        {
            Func<Uri, IObservable<BitmapSource>> resolveImage = uri =>
                Observable.Defer(() =>
                {
                    var resourcePath = "pack://application:,,,/GitHub;component/" + uri;
                    return Observable.Return(App.CreateBitmapImage(resourcePath));
                });

            return emojiCache.GetEmojis()
                .Where(emoji => !String.IsNullOrEmpty(emoji.Name)) // Just being extra cautious.
                .Select(emoji => new AutoCompleteSuggestion(emoji.Name, resolveImage(emoji.IconKey), ":", ":"))
                .ToObservable();
        }

        public string Prefix { get { return ":"; } }
    }
}
