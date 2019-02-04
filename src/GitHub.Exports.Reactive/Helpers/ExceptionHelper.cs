using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive.Linq;

#pragma warning disable CA1720 // Identifier contains type name

namespace GitHub.Helpers
{
    public static class ExceptionHelper
    {
        public static IObservable<T> ObservableThrowKeyNotFoundException<T>(string key, Exception innerException = null)
        {
            return Observable.Throw<T>(
                new KeyNotFoundException(String.Format(CultureInfo.InvariantCulture,
                "The given key '{0}' was not present in the cache.", key), innerException));
        }

        public static IObservable<T> ObservableThrowObjectDisposedException<T>(string obj, Exception innerException = null)
        {
            return Observable.Throw<T>(
                new ObjectDisposedException(String.Format(CultureInfo.InvariantCulture,
                "The cache '{0}' was disposed.", obj), innerException));
        }

        public static IObservable<T> ObservableThrowInvalidOperationException<T>(string obj, Exception innerException = null)
        {
            return Observable.Throw<T>(
                new InvalidOperationException(obj, innerException));
        }
    }
}
