using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    public class ViewerRepositoriesModel
    {
        public string Owner { get; set; }
        public IReadOnlyList<RepositoryListItemModel> Repositories { get; set; }
        public IDictionary<string, IReadOnlyList<RepositoryListItemModel>> Organizations { get; set; }
    }
}
