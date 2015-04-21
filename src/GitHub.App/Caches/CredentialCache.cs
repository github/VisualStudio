using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Akavache;
using GitHub.Helpers;
using GitHub.Authentication.CredentialManagement;

namespace GitHub.Caches
{
    public class CredentialCache : ISecureBlobCache, IObjectBlobCache
    {
        public IScheduler Scheduler { get; protected set; }

        readonly AsyncSubject<Unit> shutdown = new AsyncSubject<Unit>();
        public IObservable<Unit> Shutdown { get { return shutdown; } }

        public IObservable<Unit> Flush()
        {
            if (disposed) return ExceptionHelper.ObservableThrowObjectDisposedException<Unit>("CredentialCache");
            return Observable.Return(Unit.Default);
        }

        public IObservable<byte[]> Get(string key)
        {
            if (disposed) return ExceptionHelper.ObservableThrowObjectDisposedException<byte[]>("CredentialCache");

            var keyHost = GetKeyHost(key);
            using (var credential = new Credential())
            {
                credential.Target = keyHost;
                credential.Type = CredentialType.Generic;
                if (credential.Load())
                    return Observable.Return(Encoding.Unicode.GetBytes(credential.Password));
            }
            return ExceptionHelper.ObservableThrowKeyNotFoundException<byte[]>(key);
        }

        public IObservable<IEnumerable<string>> GetAllKeys()
        {
            throw new NotImplementedException();
        }

        public IObservable<DateTimeOffset?> GetCreatedAt(string key)
        {
            throw new NotImplementedException();
        }

        public IObservable<Unit> Insert(string key, byte[] data, DateTimeOffset? absoluteExpiration = default(DateTimeOffset?))
        {
            return ExceptionHelper.ObservableThrowInvalidOperationException<Unit>(key);
        }

        public IObservable<Unit> Invalidate(string key)
        {
            if (disposed) return ExceptionHelper.ObservableThrowObjectDisposedException<Unit>("CredentialCache");

            var keyGit = GetKeyGit(key);
            if (!DeleteKey(keyGit))
                return ExceptionHelper.ObservableThrowKeyNotFoundException<Unit>(keyGit);

            var keyHost = GetKeyHost(key);
            if (!DeleteKey(keyHost))
                return ExceptionHelper.ObservableThrowKeyNotFoundException<Unit>(keyHost);

            return Observable.Return(Unit.Default);
        }

        public IObservable<Unit> InvalidateAll()
        {
            throw new NotImplementedException();
        }

        public IObservable<Unit> Vacuum()
        {
            throw new NotImplementedException();
        }

        // TODO: Use SecureString
        public IObservable<Unit> InsertObject<T>(string key, T value, DateTimeOffset? absoluteExpiration = default(DateTimeOffset?))
        {
            if (disposed) return ExceptionHelper.ObservableThrowObjectDisposedException<Unit>("CredentialCache");

            Tuple<string, string> values = value as Tuple<string, string>;
            if (values == null)
                return ExceptionHelper.ObservableThrowInvalidOperationException<Unit>(key);

            var keyGit = GetKeyGit(key);
            if (!SaveKey(keyGit, values.Item1, values.Item2))
                return ExceptionHelper.ObservableThrowInvalidOperationException<Unit>(keyGit);

            var keyHost = GetKeyHost(key);
            if (!SaveKey(keyHost, values.Item1, values.Item2))
                return ExceptionHelper.ObservableThrowInvalidOperationException<Unit>(keyGit);

            return Observable.Return(Unit.Default);
        }

        public IObservable<T> GetObject<T>(string key)
        {
            if (typeof(T) == typeof(Tuple<string, string>))
                return (IObservable<T>) GetTuple(key);
            return ExceptionHelper.ObservableThrowInvalidOperationException<T>(key);
        }

        IObservable<Tuple<string, string>> GetTuple(string key)
        {
            if (disposed) return ExceptionHelper.ObservableThrowObjectDisposedException<Tuple<string, string>>("CredentialCache");

            var keyHost = GetKeyHost(key);
            var ret = GetKey(keyHost);
            if (ret != null)
                return Observable.Return(ret);
            return ExceptionHelper.ObservableThrowKeyNotFoundException<Tuple<string, string>>(keyHost);
        }

        public IObservable<IEnumerable<T>> GetAllObjects<T>()
        {
            throw new NotImplementedException();
        }

        public IObservable<DateTimeOffset?> GetObjectCreatedAt<T>(string key)
        {
            throw new NotImplementedException();
        }

        public IObservable<Unit> InvalidateObject<T>(string key)
        {
            if (disposed) return ExceptionHelper.ObservableThrowObjectDisposedException<Unit>("CredentialCache");

            var keyGit = GetKeyGit(key);
            if (!DeleteKey(keyGit))
                return ExceptionHelper.ObservableThrowKeyNotFoundException<Unit>(keyGit);

            var keyHost = GetKeyHost(key);
            if (!DeleteKey(keyHost))
                return ExceptionHelper.ObservableThrowKeyNotFoundException<Unit>(key);
            
            return Observable.Return(Unit.Default);
        }

        public IObservable<Unit> InvalidateAllObjects<T>()
        {
            throw new NotImplementedException();
        }

        static string FormatKey(string key)
        {
            if (key.StartsWith("login:", StringComparison.Ordinal))
                key = key.Substring("login:".Length);
            return key;
        }

        static string GetKeyGit(string key)
        {
            key = FormatKey(key);
            // it appears this is how MS expects the host key
            if (!key.StartsWith("git:", StringComparison.Ordinal))
                key = "git:" + key;
            if (key.EndsWith("/", StringComparison.Ordinal))
                key = key.Substring(0, key.Length - 1);
            return key;
        }

        static string GetKeyHost(string key)
        {
            key = FormatKey(key);
            if (key.StartsWith("git:", StringComparison.Ordinal))
                key = key.Substring("git:".Length);
            if (!key.EndsWith("/", StringComparison.Ordinal))
                key += '/';
            return key;
        }

        static bool DeleteKey(string key)
        {
            using (var credential = new Credential())
            {
                credential.Target = key;
                if (!credential.Load())
                    return false;
                return credential.Delete();
            }
        }

        static bool SaveKey(string key, string user, string pwd)
        {
            using (var credential = new Credential(user, pwd, key))
            {
                credential.Type = CredentialType.Generic;
                credential.PersistenceType = PersistenceType.LocalComputer;
                return credential.Save();
            }
        }

        static Tuple<string, string> GetKey(string key)
        {
            using (var credential = new Credential())
            {
                credential.Target = key;
                credential.Type = CredentialType.Generic;
                if (credential.Load())
                    return new Tuple<string, string>(credential.Username, credential.Password);
                return null;
            }
        }

        bool disposed;
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;

                Scheduler = null;
                shutdown.OnNext(Unit.Default);
                shutdown.OnCompleted();
                disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
