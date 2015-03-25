using GitHub.Services;
using System;
using System.Diagnostics;

namespace GitHub.Extensions
{
    public static class VSExtensions
    {
        public static T TryGetService<T>(this IServiceProvider serviceProvider) where T : class
        {
            var ui = serviceProvider as IUIProvider;
            if (ui != null)
                return ui.TryGetService<T>();
            else
            {
                try
                {
                    return serviceProvider.GetService(typeof(T)) as T;
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                }
                return null;
            }
        }
    }
}
