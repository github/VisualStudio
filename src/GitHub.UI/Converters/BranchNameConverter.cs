using GitHub.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GitHub.UI.Converters
{
    public class BranchNameConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var branch = (IBranch)values[0];
            var remoteRepo = (IRemoteRepositoryModel)branch.Repository;
            var activeRepo = (IRepositoryModel)values[1];
           
            if (remoteRepo.Parent == null && !String.Equals(activeRepo.Owner, remoteRepo.Owner))
            {
                return remoteRepo.Owner + ":" + branch.Name;
            }

            return branch.DisplayName;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
