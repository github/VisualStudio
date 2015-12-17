using GitHub.Collections;
using GitHub.Primitives;
using GitHub.UI;
using System;
using System.ComponentModel;

namespace GitHub.Models
{
    public interface ISimpleRepositoryModel : INotifyPropertyChanged,
        ICopyable<ISimpleRepositoryModel>,
        IEquatable<ISimpleRepositoryModel>,
        IComparable<ISimpleRepositoryModel>
    {
        string Name { get; }
        UriString CloneUrl { get; }
        string LocalPath { get; }
        Octicon Icon { get; }

        void SetIcon(bool isPrivate, bool isFork);

        /// <summary>
        /// Updates the url information based on the local path
        /// </summary>
        void Refresh();
    }
}
