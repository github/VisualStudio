using GitHub.Exports;
using NullGuard;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.PRCreation)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    class PullRequestCreationViewModel : BaseViewModel, IPullRequestCreationViewModel
    {
    }
}
