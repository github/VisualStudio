using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using GitHub.Extensions;
using GitHub.Helpers;
using ReactiveUI;

namespace GitHub.Models
{
    public class AutoCompleteSuggestion
    {
        readonly string prefix;
        readonly string suffix;
        readonly string[] descriptionWords;

        public AutoCompleteSuggestion(string name, string description, string prefix)
            : this(name, description, Observable.Return<BitmapSource>(null), prefix)
        {
        }

        public AutoCompleteSuggestion(string name, string description, IObservable<BitmapSource> image, string prefix)
            : this(name, description, image, prefix, null)
        {
        }

        public AutoCompleteSuggestion(string name, IObservable<BitmapSource> image, string prefix, string suffix)
            : this(name, null, image, prefix, suffix)
        {
        }

        public AutoCompleteSuggestion(string name, string description, IObservable<BitmapSource> image, string prefix, string suffix)
        {
            Guard.ArgumentNotEmptyString(name, "name");
            Guard.ArgumentNotEmptyString(prefix, "prefix"); // Suggestions have to have a triggering prefix.
            Guard.ArgumentNotNull(image, "image");

            Name = name;
            Description = description;
            if (image != null)
            {
                image = image.ObserveOn(RxApp.MainThreadScheduler);
            }
            Image = image;

            this.prefix = prefix;
            this.suffix = suffix;

            // This is pretty naive, but since the Description is currently limited to a user's FullName,
            // This is fine. When we add #issue completion, we may need to fancy this up a bit.
            descriptionWords = (description ?? String.Empty)
                .Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// The name to display in the autocomplete list box. This should not have the "@" or ":" characters around it.
        /// </summary>
        public string Name { get; private set; }

        public string Description { get; private set; }

        public IObservable<BitmapSource> Image { get; private set; }

        protected IReadOnlyCollection<string> DescriptionWords { get { return descriptionWords; } }

        // What gets autocompleted.
        public override string ToString()
        {
            return prefix + Name + suffix;
        }

        /// <summary>
        /// Used to determine if the suggestion matches the text and if so, how it should be sorted. The larger the 
        /// rank, the higher it sorts.
        /// </summary>
        /// <remarks>
        /// For mentions we sort suggestions in the following order:
        /// 
        /// 1. Login starts with text
        /// 2. Component of Name starts with text (split name by spaces, then match each word)
        /// 
        /// Non matches return -1. The secondary sort is by Login ascending.
        /// </remarks>
        /// <param name="text">The suggestion text to match</param>
        /// <returns>-1 for non-match and the sort order described in the remarks for matches</returns>
        public virtual int GetSortRank(string text)
        {
            Guard.ArgumentNotNull(text, "text");

            return Name.StartsWith(text, StringComparison.OrdinalIgnoreCase)
                    ? 1
                    : descriptionWords.Any(word => word.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                        ? 0
                        : -1;
        }
    }
}
