using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.VisualStudio.Helpers
{
    public static class PropertyNotifierExtensions
    {
        public static void RaisePropertyChange<TSender>(this TSender This, [CallerMemberName] string propertyName = null)
           where TSender : INotifyPropertySource
        {
            This.RaisePropertyChanged(propertyName);
        }
    }
}
