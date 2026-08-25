namespace Olive.Microservices.Hub.Domain.Theme
{
    using Microsoft.Extensions.Configuration;
    using Olive;
    using Olive.Microservices.Hub.Domain.Theme.Types;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>Reads a role based url straight out of configuration instead of leaving the
    /// configuration binder to work out whether the setting was written as a string or as an object.
    ///
    /// The binder makes that call from the property's type, not from the value: a type carrying a
    /// TypeConverter that accepts a string is treated as a scalar and its children are never read.
    /// FS.Shared.Website's %DOMAIN% pass, which runs in UAT and Production only, assigns null to
    /// every non leaf configuration key. That creates a key where there was none, and from then on
    /// the binder sees these settings as scalars holding no value and binds them to null, silently
    /// dropping Default and Roles. Reading the children here keeps both shapes working wherever
    /// that pass has run.</summary>
    internal static class RoleBasedUrlReader
    {
        public static TUrl? Read<TUrl>(IConfigurationSection section) where TUrl : RoleBasedUrl, new()
        {
            // "SidebarProfileUrl": "/person/%EMAIL%"
            if (section.GetChildren().None())
                return section.Value.IsEmpty() ? null : new TUrl { Default = section.Value };

            // "SidebarProfileUrl": { "Default": ..., "Roles": { ... } }
            return new TUrl
            {
                Default = section[nameof(RoleBasedUrl.Default)],
                Roles = ReadRoles(section.GetSection(nameof(RoleBasedUrl.Roles)))
            };
        }

        static Dictionary<string, RoleUrl>? ReadRoles(IConfigurationSection section)
        {
            var roles = section.GetChildren().ToArray();
            if (roles.None()) return null;

            return roles.ToDictionary(x => x.Key, ReadRole);
        }

        static RoleUrl ReadRole(IConfigurationSection section)
        {
            // "Student": "/profile/s_%ID%"
            if (section.GetChildren().None()) return new RoleUrl { Url = section.Value };

            // "Student": { "Url": "/profile/s_%ID%", "Priority": 1 }
            return new RoleUrl
            {
                Url = section[nameof(RoleUrl.Url)],
                Priority = section.GetValue<int>(nameof(RoleUrl.Priority))
            };
        }
    }
}
