
namespace Olive.Microservices.Hub.Domain.Theme
{
    using Olive.Microservices.Hub.Domain.Theme.Contracts;
    using Microsoft.Extensions.DependencyInjection;

    public static class ThemeExtensions
    {
        internal static void AddThemes(this IServiceCollection @this)
        {
            @this.AddScoped<IThemeProvider, ThemeProvider>();
            @this.AddScoped<IThemeValidations, ThemeValidations>();
        }
    }
}