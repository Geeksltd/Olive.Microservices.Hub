namespace Olive.Microservices.Hub.Domain.Theme
{
    using Microsoft.AspNetCore.Http;
    using Olive.Microservices.Hub.Domain.Theme.Contracts;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
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
            var types = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    types.AddRange(
                        assembly.GetTypes()
                            .Where(t => t.IsClass && typeof(IThemeValidator).IsAssignableFrom(t)));
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Console.WriteLine($"Failed assembly: {assembly.FullName}");

                    foreach (var loaderEx in ex.LoaderExceptions)
                        Console.WriteLine(loaderEx);
                }
            }

            _validators = types
                .Select(t => Activator.CreateInstance(t) as IThemeValidator)
                .Where(v => v != null)
                .Cast<IThemeValidator>()
                .ToList();
        }
    }
}