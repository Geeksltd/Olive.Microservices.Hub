using Olive.Microservices.Hub.Domain.Theme.Contracts;
using Olive;

namespace Olive.Microservices.Hub.Domain.Theme
{
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.Extensions.DependencyInjection;
    using System.Collections.Generic;
    public class ThemeViewLocationExpander : Olive.Mvc.ViewLocationExpander
    {
        private const string ValueKey = "theme";

        public override IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var baseLocations = base.ExpandViewLocations(context, viewLocations);

            context.Values.TryGetValue(ValueKey, out var themeName);

            if (themeName.IsEmpty()) return baseLocations;

            var partialViewLocationFormats = new[] {
                "~/Themes/"+ themeName + "/Modules/{0}.cshtml",
                "~/Themes/"+ themeName + "/Layouts/{0}.cshtml",
                "~/Themes/"+ themeName + "/Shared/{0}.cshtml",
                "~/Themes/"+ themeName + "/Modules/Components/{1}/Default.cshtml"};

            var viewLocationFormats = new[] {
                "~/Themes/"+ themeName + "/Modules/{0}.cshtml",
                "~/Themes/"+ themeName + "/Pages/{1}.cshtml",
                "~/Themes/"+ themeName + "/Modules/{1}.cshtml",
                "~/Themes/"+ themeName + "/Shared/{0}.cshtml",
                "~/Themes/"+ themeName + "/Modules/Components/{1}/Default.cshtml"};

            return context.IsMainPage ? viewLocationFormats.Union(baseLocations) : partialViewLocationFormats.Union(baseLocations);
        }

        public override void PopulateValues(ViewLocationExpanderContext context)
        {
            var themeProvider = context.ActionContext.HttpContext.RequestServices.GetService<IThemeProvider>();

            var theme = themeProvider?.GetCurrentTheme().GetAwaiter().GetResult();
            var themeName = theme?.Name.ToPascalCaseId();

            context.Values[ValueKey] = themeName;
        }
    }
}