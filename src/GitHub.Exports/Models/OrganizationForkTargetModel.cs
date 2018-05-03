using GitHub.Models;

namespace GitHub.Models
{
    public class OrganizationDetailsModel: IOrganizationDetailsModel
    {
        public string Name { get; set; }

        public bool ViewerCanCreateProjects { get; set; }
    }
}