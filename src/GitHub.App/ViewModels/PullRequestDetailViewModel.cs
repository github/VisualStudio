using GitHub.Exports;
using GitHub.UI;
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
    [ExportViewModel(ViewType = UIViewType.PRDetail)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    class PullRequestDetailViewModel : BaseViewModel, IPullRequestDetailViewModel
    {
        public override void Initialize([AllowNull] ViewWithData data)
        {
            System.Windows.MessageBox.Show(String.Format(CultureInfo.InvariantCulture, "{0}", data.Data));
        }
    }
}
