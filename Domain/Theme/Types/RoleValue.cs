namespace Olive.Microservices.Hub.Domain.Theme.Types
{
    /// <summary>One entry of a role based setting's Roles map: the value that role gets, and how it
    /// ranks against the other configured roles the same user holds. It can be configured either as
    /// an object (Value + Priority) or as a plain value, which is read as the Value.</summary>
    public class RoleValue<TValue>
    {
        public TValue? Value { get; set; }

        /// <summary>Ranks this entry when the user holds more than one of the configured roles: the
        /// lowest priority wins. Configuration cannot preserve the order the roles were written in,
        /// so entries left at the default 0 fall back to key order, which is alphabetical.</summary>
        public int Priority { get; set; }

        public override string ToString()
        {
            return $"{Value} ({Priority})";
        }
    }
}
