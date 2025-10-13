
namespace Olive.Microservices.Hub.Domain.Theme
{
    using Microsoft.Extensions.DependencyInjection;
    using Olive.Microservices.Hub.Domain.Theme.Contracts;

    public static class ThemeExtensions
    {
        internal static void AddThemes(this IServiceCollection @this)
        {
            @this.AddScoped<IThemeProvider, ThemeProvider>();
            @this.AddScoped<IThemeValidations, ThemeValidations>();
            @this.AddScoped<IThemeLoginLoggers, ThemeLoginLoggers>();
            @this.AddScoped<IInvalidMagicLinkLoggers, InvalidMagicLinkLoggers>();
        }
    }
}