using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.Services
{
    public interface ITeamExplorerServiceHolder
    {
        IServiceProvider ServiceProvider { get; }
        void SetServiceProvider(IServiceProvider provider);
        void ClearServiceProvider(IServiceProvider provider);
        void Subscribe(object who, Action<IServiceProvider> handler);
        void Unsubscribe(object who);
    }
}
