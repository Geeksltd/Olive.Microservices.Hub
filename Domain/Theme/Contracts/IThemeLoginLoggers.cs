using Olive.Microservices.Hub.Domain.Theme.LoginLoggers;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub.Domain.Theme.Contracts
{
    public interface IThemeLoginLoggers
    {
        Task Log(Theme.Types.Theme currentTheme, string email, LoginLogStatus status, string? message = null);
    }
}