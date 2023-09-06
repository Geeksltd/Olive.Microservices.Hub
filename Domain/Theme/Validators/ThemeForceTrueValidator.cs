namespace Olive.Microservices.Hub.Domain.Theme.Validators
{
    using Olive.Microservices.Hub.Domain.Theme.Contracts;
    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;

    public class ThemeForceTrueValidator : IThemeValidator
    {
        public string Name => "ForceTrue";

        public Task<bool> Validate(HttpContext? httpContext, Types.Theme theme)
        {
            return Task.FromResult(true);
        }
    }
}