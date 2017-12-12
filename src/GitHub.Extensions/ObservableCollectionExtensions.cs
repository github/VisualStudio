using System;
using System.Collections;
using System.Collections.Specialized;

namespace GitHub.Extensions
{
    public static class ObservableCollectionExtensions
    {
        /// <summary>
        /// Invokes an action for each item in a collection and subsequently each item added or
        /// removed from the collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection items.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="added">
        /// An action called initially for each item in the collection and subsequently for each
        /// item added to the collection.
        /// </param>
        /// <param name="removed">
        /// An action called for each item removed from the collection.
        /// </param>
        /// <param name="reset">
        /// An action called when the collection is reset. This will be followed by calls to 
        /// <paramref name="added"/> for each item present in the collection after the reset.
        /// </param>
        /// <returns>A disposable used to terminate the subscription.</returns>
        public static IDisposable ForEachItem<T>(
            this IReadOnlyObservableCollection<T> collection,
            Action<T> added,
            Action<T> removed,
            Action reset)
        {
            Action<IList> add = items =>
            {
                foreach (T item in items)
                {
                    added(item);
                }
            };

            Action<IList> remove = items =>
            {
                for (var i = items.Count - 1; i >= 0; --i)
                {
                    removed((T)items[i]);
                }
            };

            NotifyCollectionChangedEventHandler handler = (_, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        add(e.NewItems);
                        break;

                    case NotifyCollectionChangedAction.Move:
                    case NotifyCollectionChangedAction.Replace:
                        remove(e.OldItems);
                        add(e.NewItems);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        remove(e.OldItems);
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        if (reset == null)
                        {
                            throw new InvalidOperationException(
                                "Reset called on collection without reset handler.");
                        }

                        reset();
                        add((IList)collection);
                        break;
                }
            };

            add((IList)collection);
            collection.CollectionChanged += handler;

            return new ActionDisposable(() => collection.CollectionChanged -= handler);
        }

        class ActionDisposable : IDisposable
        {
            Action dispose;

            public ActionDisposable(Action dispose)
            {
                this.dispose = dispose;
            }

            public void Dispose()
            {
                dispose();
            }
        }
    }
}
