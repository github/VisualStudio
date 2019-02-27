using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    public class OrganizationRepositoriesModel
    {
        public ActorModel Organization { get; set; }
        public IReadOnlyList<RepositoryListItemModel> Repositories { get; set; }
    }
}
