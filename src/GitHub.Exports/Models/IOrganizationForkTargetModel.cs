namespace GitHub.Models
{
    public interface IOrganizationDetailsModel
    {
        string Name { get; set; }
        bool ViewerCanCreateProjects { get; set; }
    }
}