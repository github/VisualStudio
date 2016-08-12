using System;
using System.Linq;
using System.Reactive.Linq;
using NullGuard;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// A view model that supports back/forward navigation of an inner content page.
    /// </summary>
    /// <typeparam name="TContent">The type of the content page view model.</typeparam>
    [NullGuard(ValidationFlags.None)]
    public class NavigatingViewModel<TContent> : ReactiveObject
    {
        private readonly ReactiveList<TContent> history = new ReactiveList<TContent>();
        private readonly ObservableAsPropertyHelper<TContent> content;
        private int index = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigatingViewModel{TContent}"/> class.
        /// </summary>
        public NavigatingViewModel()
        {
            var pos = this.WhenAnyValue(
                x => x.Index,
                x => x.history.Count,
                (i, c) => new { Index = i, Count = c });

            content = pos
                .Where(x => x.Index < x.Count)
                .Select(x => x.Index != -1 ? history[x.Index] : default(TContent))
                .StartWith(default(TContent))
                .ToProperty(this, x => x.Content);

            NavigateBack = ReactiveCommand.Create(pos.Select(x => x.Index > 0));
            NavigateBack.Subscribe(_ => Back());
            NavigateForward = ReactiveCommand.Create(pos.Select(x => x.Index < x.Count - 1));
            NavigateForward.Subscribe(_ => Forward());
        }

        /// <summary>
        /// Gets or sets the current content page.
        /// </summary>
        public TContent Content => content.Value;

        /// <summary>
        /// Gets a command that navigates back in the history.
        /// </summary>
        public ReactiveCommand<object> NavigateBack { get; }

        /// <summary>
        /// Gets a command that navigates forwards in the history.
        /// </summary>
        public ReactiveCommand<object> NavigateForward { get; }

        /// <summary>
        /// Gets or sets the current index in the history list.
        /// </summary>
        private int Index
        {
            get { return index; }
            set { this.RaiseAndSetIfChanged(ref index, value); }
        }

        /// <summary>
        /// Navigates to a new page.
        /// </summary>
        /// <param name="page">The page view model.</param>
        public void NavigateTo(TContent page)
        {
            Extensions.Guard.ArgumentNotNull(page, nameof(page));

            if (index < history.Count - 1)
            {
                history.RemoveRange(index + 1, history.Count - (index + 1));
            }

            history.Add(page);
            ++Index;
        }

        /// <summary>
        /// Navigates back if possible.
        /// </summary>
        /// <returns>True if there was a page to navigate back to.</returns>
        public bool Back()
        {
            if (index == 0)
                return false;
            --Index;
            return true;
        }

        /// <summary>
        /// Navigates forwards if possible.
        /// </summary>
        /// <returns>True if there was a page to navigate forwards to.</returns>
        public bool Forward()
        {
            if (index >= history.Count - 1)
                return false;
            ++Index;
            return true;
        }

        /// <summary>
        /// Clears the current page and all history .
        /// </summary>
        public void Clear()
        {
            Index = -1;
            history.Clear();
        }
    }
}
