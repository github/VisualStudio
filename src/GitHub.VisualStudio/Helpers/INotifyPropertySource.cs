using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.VisualStudio
{
    public interface INotifyPropertySource
    {
        void RaisePropertyChanged(string propertyName);
    }
}
