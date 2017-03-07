using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.UI
{
    public interface IHasDone
    {
        IObservable<object> Done { get; }
    }
}
