namespace Olive.Microservices.Hub
{
    using Olive;
    using Olive.Entities;

    /// <summary>Provides the ability to inject filter business logic into database queries.</summary>
    public static partial class DatabaseQueryExtensions
    {
        static IDatabase Database => Context.Current.Database();
    }
}