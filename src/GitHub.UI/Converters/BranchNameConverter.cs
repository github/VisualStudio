using GitHub.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GitHub.UI.Converters
{
    public class BranchNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var branch = (IBranch)value;
            var repo = (IRemoteRepositoryModel)branch.Repository;

            if (repo.Parent != null )
            {
                return repo.Parent.Owner + ":" + branch.Name;
            }

            return branch.DisplayName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
