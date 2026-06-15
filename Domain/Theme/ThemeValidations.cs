namespace Olive.Microservices.Hub.Domain.Theme
{
    using Microsoft.AspNetCore.Http;
    using Olive.Microservices.Hub.Domain.Theme.Contracts;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class ThemeValidations : IThemeValidations
    {

        static List<IThemeValidator> _validators = new();
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ThemeValidations(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> IsValid(Types.Theme theme)
        {
            if (_validators.Count == 0) { LoadMap(); }

            var validator = _validators.SingleOrDefault(a => a.Name == theme.ValidationFunction);
            if (validator is null)
                return false;

            return await validator.Validate(_httpContextAccessor.HttpContext, theme);
        }

        void LoadMap()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a =>
                {
                    var name = a.GetName().Name;
                    return name is "website" or "Olive.Microservices.Hub";
                });

            var types = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    typeof(IThemeValidator).IsAssignableFrom(t))
                .ToList();

            _validators = types
                .Select(t => Activator.CreateInstance(t) as IThemeValidator)
                .Where(v => v != null)
                .Cast<IThemeValidator>()
                .ToList();
        }
    }
}