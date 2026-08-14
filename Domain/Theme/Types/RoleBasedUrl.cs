namespace Olive.Microservices.Hub.Domain.Theme.Types
{
    using System.Collections.Generic;

    /// <summary>A theme url that can vary by the current user's role.
    /// The first Roles entry matching any of the user's roles wins, otherwise Default is used.</summary>
    public abstract class RoleBasedUrl
    {
        public string? Default { get; set; }
        public Dictionary<string, string>? Roles { get; set; }

        public override string ToString()
        {
            return Default + $" ({Roles?.Count ?? 0} roles)";
        }
    }
}
