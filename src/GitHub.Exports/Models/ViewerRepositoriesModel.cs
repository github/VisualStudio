using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    public class ViewerRepositoriesModel
    {
        public IReadOnlyList<RepositoryListItemModel> Repositories { get; set; }
        public IDictionary<string, IReadOnlyList<RepositoryListItemModel>> OrganizationRepositories { get; set; }
    }
}
