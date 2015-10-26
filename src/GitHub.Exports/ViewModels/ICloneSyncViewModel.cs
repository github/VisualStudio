using GitHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.ViewModels
{
    public interface ICloneSyncViewModel
    {
        IObservable<bool> Sync(ISimpleRepositoryModel repo);
    }
}
