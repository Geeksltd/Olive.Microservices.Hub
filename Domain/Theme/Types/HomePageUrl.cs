namespace Olive.Microservices.Hub.Domain.Theme.Types
{
    /// <summary>The page a user lands on at the site root, optionally varying by their role.
    /// It can be configured either as an object (Default + Roles) or, as before, as a plain
    /// string which is read as the Default.</summary>
    public class HomePageUrl : RoleBasedUrl
    {
    }
}
