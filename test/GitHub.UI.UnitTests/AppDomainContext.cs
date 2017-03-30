using System;

namespace GitHub.UI.UnitTests
{
    public class AppDomainContext : IDisposable
    {
        AppDomain domain;

        public AppDomainContext(AppDomainSetup setup)
        {
            var friendlyName = GetType().FullName;
            domain = AppDomain.CreateDomain(friendlyName, null, setup);
        }

        public T CreateInstance<T>()
        {
            return (T)domain.CreateInstanceFromAndUnwrap(typeof(T).Assembly.CodeBase, typeof(T).FullName);
        }

        public void Dispose()
        {
            AppDomain.Unload(domain);
        }
    }
}
