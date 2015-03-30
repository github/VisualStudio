using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.Models
{
    public interface IConnectionManager
    {
        ObservableCollection<IConnection> Connections { get; }
    }
}
