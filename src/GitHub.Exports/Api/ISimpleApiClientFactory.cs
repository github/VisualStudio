using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.Api
{
    public interface ISimpleApiClientFactory
    {
        ISimpleApiClient Create(Uri repoUrl);
    }
}
