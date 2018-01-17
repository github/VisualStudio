using System;
using System.ComponentModel;
using GitHub.Models;

namespace GitHub.Services
{
    public interface ITeamExplorerContext : INotifyPropertyChanged
    {
        ILocalRepositoryModel ActiveRepository { get; }
        event EventHandler StatusChanged;
    }
}
