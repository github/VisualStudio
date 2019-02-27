using System;
using System.Collections.Generic;
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
        ScopesCollection scopes;
        bool isLoggedIn;
        bool isLoggingIn;
        Exception connectionError;

        public Connection(
            HostAddress hostAddress,
            string username)
        {
            this.username = username;
            HostAddress = hostAddress;
            isLoggedIn = false;
            isLoggingIn = true;
        }

        public Connection(
            HostAddress hostAddress,
            Octokit.User user,
            ScopesCollection scopes)
        {
            HostAddress = hostAddress;
            this.user = user;
            this.scopes = scopes;
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
        public ScopesCollection Scopes
        {
            get => scopes;
            private set => RaiseAndSetIfChanged(ref scopes, value);
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
            Scopes = null;
        }

        public void SetError(Exception e)
        {
            ConnectionError = e;
            IsLoggingIn = false;
            IsLoggedIn = false;
            Scopes = null;
        }

        public void SetSuccess(Octokit.User user, ScopesCollection scopes)
        {
            User = user;
            Scopes = scopes;
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
