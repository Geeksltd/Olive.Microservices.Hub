using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub.Domain.Theme.Contracts
{
    public interface IThemeValidator
    {
        string Name { get; }
        Task<bool> Validate(HttpContext? httpContext, Types.Theme theme);
    }
}