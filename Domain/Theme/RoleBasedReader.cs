namespace Olive.Microservices.Hub.Domain.Theme
{
    using Microsoft.Extensions.Configuration;
    using Olive;
    using Olive.Microservices.Hub.Domain.Theme.Types;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>Reads a role based setting straight out of configuration instead of leaving the
    /// configuration binder to work out whether the setting was written as a plain value or as an
    /// object.
    ///
    /// The binder makes that call from the property's type, not from the value: a type carrying a
    /// TypeConverter that accepts a string is treated as a scalar and its children are never read.
    /// FS.Shared.Website's %DOMAIN% pass, which runs in UAT and Production only, assigns null to
    /// every non leaf configuration key. That creates a key where there was none, and from then on
    /// the binder sees these settings as scalars holding no value and binds them to null, silently
    /// dropping Default and Roles. Reading the children here keeps both shapes working wherever
    /// that pass has run.</summary>
    internal static class RoleBasedReader
    {
        public static TSetting? Read<TSetting, TValue>(IConfigurationSection section)
            where TSetting : RoleBased<TValue>, new()
        {
            // "SidebarProfileUrl": "/person/%EMAIL%"   |   "HideEveryThingMenuItem": true
            if (section.GetChildren().None())
                return section.Value.IsEmpty() ? null : new TSetting { Default = section.Get<TValue>() };

            // "SidebarProfileUrl": { "Default": ..., "Roles": { ... } }
            return new TSetting
            {
                Default = section.GetSection(nameof(RoleBased<TValue>.Default)).Get<TValue>(),
                Roles = ReadRoles<TValue>(section.GetSection(nameof(RoleBased<TValue>.Roles)))
            };
        }

        static Dictionary<string, RoleValue<TValue>>? ReadRoles<TValue>(IConfigurationSection section)
        {
            var roles = section.GetChildren().ToArray();
            if (roles.None()) return null;

            return roles.ToDictionary(x => x.Key, ReadRole<TValue>);
        }

        static RoleValue<TValue> ReadRole<TValue>(IConfigurationSection section)
        {
            // "Student": "/profile/s_%ID%"   |   "Admin": false
            if (section.GetChildren().None())
                return new RoleValue<TValue> { Value = section.Get<TValue>() };

            // "Student": { "Value": "/profile/s_%ID%", "Priority": 1 }
            return new RoleValue<TValue>
            {
                Value = section.GetSection(nameof(RoleValue<TValue>.Value)).Get<TValue>(),
                Priority = section.GetValue<int>(nameof(RoleValue<TValue>.Priority))
            };
        }
    }
}
