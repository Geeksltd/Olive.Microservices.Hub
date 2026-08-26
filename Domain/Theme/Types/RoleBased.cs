namespace Olive.Microservices.Hub.Domain.Theme.Types
{
    using System.Collections.Generic;

    /// <summary>A theme setting that can vary by the current user's role.
    /// Of the Roles entries whose role the user holds the lowest Priority wins, otherwise Default is
    /// used. Each entry is either a plain value or an object with Value and Priority.</summary>
    public abstract class RoleBased<TValue>
    {
        public TValue? Default { get; set; }
        public Dictionary<string, RoleValue<TValue>>? Roles { get; set; }

        public override string ToString()
        {
            return $"{Default} ({Roles?.Count ?? 0} roles)";
        }
    }
}
