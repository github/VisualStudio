using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.ViewModels
{
    public interface ICanBeBusy
    {
        bool IsBusy { get; }
    }
}
