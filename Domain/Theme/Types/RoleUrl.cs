namespace Olive.Microservices.Hub.Domain.Theme.Types
{
    /// <summary>One entry of a role based url's Roles map: the url that role lands on, and how it
    /// ranks against the other configured roles the same user holds. It can be configured either as
    /// an object (Url + Priority) or as a plain string, which is read as the Url.</summary>
    public class RoleUrl
    {
        public string? Url { get; set; }

        /// <summary>Ranks this entry when the user holds more than one of the configured roles: the
        /// lowest priority wins. Configuration cannot preserve the order the roles were written in,
        /// so entries left at the default 0 fall back to key order, which is alphabetical.</summary>
        public int Priority { get; set; }

        public override string ToString()
        {
            return $"{Url} ({Priority})";
        }
    }
}
