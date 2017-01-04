using GitHub.UI;
using System;

namespace GitHub.ViewModels
{
    public interface IServiceProviderAware
    {
        void Initialize(IServiceProvider serviceProvider);
    }

    public interface IViewHost
    {
        void ShowView(ViewWithData data);
    }
}
