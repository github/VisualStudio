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
            var branch = values[0] as IBranch;
            var activeRepo = values[1] as IRepositoryModel;

            if (branch != null && activeRepo != null)
            {
                var repo = (IRemoteRepositoryModel)branch.Repository;

                if (repo.Parent == null && activeRepo.Owner != repo.Owner)
                {
                    return repo.Owner + ":" + branch.Name;
                }

                return branch.DisplayName;
            }
            else
            {
                return values[0]?.ToString();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
