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
            var branch = values.OfType<BranchModel>().FirstOrDefault();
            var activeRepo = values.OfType<RepositoryModel>().FirstOrDefault();

            if (branch != null && activeRepo != null)
            {
                var repo = (RemoteRepositoryModel)branch.Repository;

                if (repo.Parent == null && activeRepo.Owner != repo.Owner)
                {
                    return repo.Owner + ":" + branch.Name;
                }

                return branch.DisplayName;
            }
            else
            {
                return values.FirstOrDefault()?.ToString();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
