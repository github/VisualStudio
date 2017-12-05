using System;

namespace GitHub.ViewModels
{
    public interface IServiceProviderAware
    {
        void Initialize(IServiceProvider serviceProvider);
    }
}
