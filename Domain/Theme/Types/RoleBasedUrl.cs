namespace Olive.Microservices.Hub.Domain.Theme.Types
{
    using System.Collections.Generic;

    /// <summary>A theme url that can vary by the current user's role.
    /// Of the Roles entries whose role the user holds the lowest Priority wins, otherwise Default is
    /// used. Each entry is either a plain url string or an object with Url and Priority.</summary>
    public abstract class RoleBasedUrl
    {
        public string? Default { get; set; }
        public Dictionary<string, RoleUrl>? Roles { get; set; }

        public override string ToString()
        {
            return Default + $" ({Roles?.Count ?? 0} roles)";
        }
    }
}
