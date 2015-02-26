using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NullGuard
{
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class AllowNullAttribute : Attribute
    {
        
    }
}
