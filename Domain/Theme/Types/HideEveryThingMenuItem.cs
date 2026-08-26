namespace Olive.Microservices.Hub.Domain.Theme.Types
{
    /// <summary>Whether the Everything item is left out of the side menu, optionally varying by the
    /// user's role. It can be configured either as an object (Default + Roles) or, as before, as a
    /// plain boolean which is read as the Default. Unset means the item is shown.</summary>
    public class HideEveryThingMenuItem : RoleBased<bool?>
    {
    }
}
