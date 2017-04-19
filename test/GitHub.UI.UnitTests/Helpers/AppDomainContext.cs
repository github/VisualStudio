using System;
using System.Reflection;
using System.Collections.Generic;

namespace GitHub.UI.Helpers.UnitTests
{
    public class AppDomainContext : IDisposable
    {
        AppDomain domain;

        public static void Invoke(AppDomainSetup setup, Action remoteAction)
        {
            using (var context = new AppDomainContext(setup))
            {
                context.Invoke(remoteAction);
            }
        }

        public AppDomainContext(AppDomainSetup setup = null)
        {
            if (setup == null)
            {
                setup = new AppDomainSetup();
                setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory; // don't use the process base dir
            }

            var friendlyName = GetType().FullName;
            domain = AppDomain.CreateDomain(friendlyName, null, setup);
        }

        public void Invoke(Action remoteAction)
        {
            var targetType = remoteAction.Target.GetType();
            var fieldValues = new Dictionary<string, object>();
            foreach (var field in targetType.GetFields())
            {
                var value = field.GetValue(remoteAction.Target);
                fieldValues[field.Name] = value;
            }

            var remove = CreateInstance<Remote>();
            var assemblyFile = targetType.Assembly.Location;
            var typeName = targetType.FullName;
            var methodName = remoteAction.Method.Name;
            remove.Invoke(assemblyFile, typeName, methodName, fieldValues);
        }

        public T CreateInstance<T>()
        {
            return (T)domain.CreateInstanceFromAndUnwrap(typeof(T).Assembly.CodeBase, typeof(T).FullName);
        }

        bool disposed;
        private void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                disposed = true;
                AppDomain.Unload(domain);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        class Remote : MarshalByRefObject
        {
            internal void Invoke(string assemblyFile, string typeName, string methodName, Dictionary<string, object> fieldValues)
            {
                var assembly = Assembly.LoadFrom(assemblyFile);
                var type = assembly.GetType(typeName);
                var obj = Activator.CreateInstance(type);
                foreach (var field in type.GetFields())
                {
                    var value = fieldValues[field.Name];
                    field.SetValue(obj, value);
                }

                var dele = (Action)Delegate.CreateDelegate(typeof(Action), obj, methodName);
                dele();
            }
        }
    }
}
