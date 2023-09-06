namespace Olive.Microservices.Hub.Domain.Theme.Types
{
    using System.Collections.Generic;

    public class SidebarProfileUrl
    {
        public string Default { get; set; }
        public Dictionary<string, string>? Roles { get; set; }

        public override string ToString()
        {
            return Default + $" ({Roles?.Count ?? 0} roles)";
        }
    }
}