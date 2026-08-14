namespace Olive.Microservices.Hub.Domain.Theme.Types
{
    using System.ComponentModel;

    /// <summary>The url of the profile link in the sidebar and footer, optionally varying by the
    /// user's role. It can be configured either as an object (Default + Roles) or as a plain
    /// string which is read as the Default.</summary>
    [TypeConverter(typeof(RoleBasedUrlConverter<SidebarProfileUrl>))]
    public class SidebarProfileUrl : RoleBasedUrl
    {
    }
}
