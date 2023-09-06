using Olive;

namespace Olive.Microservices.Hub.Domain.Theme
{
    using Olive.Microservices.Hub.Domain.Theme.Contracts;
    using Microsoft.AspNetCore.Http;
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
            var types = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(a => a.IsClass && typeof(IThemeValidator).IsAssignableFrom(a))
                .ToList();

            _validators = types
                .Select(a => Activator.CreateInstance(a) as IThemeValidator)
                .Where(a => a is not null)
                .Cast<IThemeValidator>()
                .ToList();
        }
    }
}