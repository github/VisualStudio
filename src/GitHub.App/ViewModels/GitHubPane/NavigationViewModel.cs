using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using GitHub.Extensions;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// A view model that supports back/forward navigation of an inner content page.
    /// </summary>
    [Export(typeof(INavigationViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class NavigationViewModel : ViewModelBase, INavigationViewModel
    {
        readonly ReactiveList<IPanePageViewModel> history;
        readonly ObservableAsPropertyHelper<IPanePageViewModel> content;
        Dictionary<IPanePageViewModel, CompositeDisposable> pageDispose;
        int index = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationViewModel"/> class.
        /// </summary>
        public NavigationViewModel()
        {
            history = new ReactiveList<IPanePageViewModel>();
            history.BeforeItemsAdded.Subscribe(BeforeItemAdded);
            history.CollectionChanged += CollectionChanged;

            var pos = this.WhenAnyValue(
                x => x.Index,
                x => x.History.Count,
                (i, c) => new { Index = i, Count = c });

            content = pos
                .Where(x => x.Index < x.Count)
                .Select(x => x.Index != -1 ? history[x.Index] : null)
                .StartWith((IPanePageViewModel)null)
                .ToProperty(this, x => x.Content);

            NavigateBack = ReactiveCommand.Create(pos.Select(x => x.Index > 0));
            NavigateBack.Subscribe(_ => Back());
            NavigateForward = ReactiveCommand.Create(pos.Select(x => x.Index < x.Count - 1));
            NavigateForward.Subscribe(_ => Forward());
        }

        /// <inheritdoc/>
        public IPanePageViewModel Content => content.Value;

        /// <inheritdoc/>
        public int Index
        {
            get { return index; }
            set { this.RaiseAndSetIfChanged(ref index, value); }
        }

        /// <inheritdoc/>
        public IReadOnlyReactiveList<IPanePageViewModel> History => history;

        /// <inheritdoc/>
        public ReactiveCommand<object> NavigateBack { get; }

        /// <inheritdoc/>
        public ReactiveCommand<object> NavigateForward { get; }

        /// <inheritdoc/>
        public void NavigateTo(IPanePageViewModel page)
        {
            Guard.ArgumentNotNull(page, nameof(page));

            if (index < history.Count - 1)
            {
                history.RemoveRange(index + 1, history.Count - (index + 1));
            }

            history.Add(page);
            ++Index;
        }

        /// <inheritdoc/>
        public bool Back()
        {
            if (index == 0)
                return false;
            --Index;
            return true;
        }

        /// <inheritdoc/>
        public bool Forward()
        {
            if (index >= history.Count - 1)
                return false;
            ++Index;
            return true;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            Index = -1;
            history.Clear();
        }

        public int RemoveAll(IPanePageViewModel page)
        {
            var count = 0;
            while (history.Remove(page)) ++count;
            return count;
        }
             
        public void RegisterDispose(IPanePageViewModel page, IDisposable dispose)
        {
            if (pageDispose == null)
            {
                pageDispose = new Dictionary<IPanePageViewModel, CompositeDisposable>();
            }

            CompositeDisposable item;

            if (!pageDispose.TryGetValue(page, out item))
            {
                item = new CompositeDisposable();
                pageDispose.Add(page, item);
            }

            item.Add(dispose);
        }

        void BeforeItemAdded(IPanePageViewModel page)
        {
            if (page.CloseRequested != null && !history.Contains(page))
            {
                RegisterDispose(
                    page,
                    page.CloseRequested.Subscribe(_ => RemoveAll(page)));
            }
        }

        void ItemRemoved(int removedIndex, IPanePageViewModel page)
        {
            if (removedIndex <= Index)
            {
                --Index;
            }

            if (!history.Contains(page))
            {
                CompositeDisposable dispose = null;

                if (pageDispose?.TryGetValue(page, out dispose) == true)
                {
                    dispose.Dispose();
                    pageDispose.Remove(page);
                }
            }
        }

        void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            using (DelayChangeNotifications())
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        for (var i = 0; i < e.OldItems.Count; ++i)
                        {
                            ItemRemoved(e.OldStartingIndex + i, (IPanePageViewModel)e.OldItems[i]);
                        }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        if (pageDispose != null)
                        {
                            foreach (var dispose in pageDispose.Values)
                            {
                                dispose.Dispose();
                            }

                            pageDispose.Clear();
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}