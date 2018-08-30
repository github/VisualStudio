using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.Services
{
    /// <summary>
    /// Represents a configured connection to a GitHub account.
    /// </summary>
    public class Connection : IConnection
    {
        string username;
        Octokit.User user;
        bool isLoggedIn;
        bool isLoggingIn;
        Exception connectionError;

        public Connection(HostAddress hostAddress)
        {
            HostAddress = hostAddress;
            isLoggedIn = false;
            isLoggingIn = true;
        }

        public Connection(
            HostAddress hostAddress,
            string username,
            Octokit.User user)
        {
            HostAddress = hostAddress;
            this.username = username;
            this.user = user;
            isLoggedIn = true;
        }

        /// <inheritdoc/>
        public HostAddress HostAddress { get; }

        /// <inheritdoc/>
        public string Username
        {
            get => username;
            private set => RaiseAndSetIfChanged(ref username, value);
        }

        /// <inheritdoc/>
        public Octokit.User User
        {
            get => user;
            private set => RaiseAndSetIfChanged(ref user, value);
        }

        /// <inheritdoc/>
        public bool IsLoggedIn
        {
            get => isLoggedIn;
            private set => RaiseAndSetIfChanged(ref isLoggedIn, value);
        }

        /// <inheritdoc/>
        public bool IsLoggingIn
        {
            get => isLoggingIn;
            private set => RaiseAndSetIfChanged(ref isLoggingIn, value);
        }

        /// <inheritdoc/>
        public Exception ConnectionError
        {
            get => connectionError;
            private set => RaiseAndSetIfChanged(ref connectionError, value);
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        public void SetLoggingIn()
        {
            ConnectionError = null;
            IsLoggedIn = false;
            IsLoggingIn = true;
            User = null;
            Username = null;
        }

        public void SetError(Exception e)
        {
            ConnectionError = e;
            IsLoggingIn = false;
            IsLoggedIn = false;
        }

        public void SetSuccess(Octokit.User user)
        {
            User = user;
            Username = user.Login;
            IsLoggingIn = false;
            IsLoggedIn = true;
        }

        void RaiseAndSetIfChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
