using Olive.Microservices.Hub.Domain.Theme.LoginLoggers;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub.Domain.Theme.Contracts
{
    public interface IThemeLoginLogger
    {
        string Name { get; }
        Task Log(string email, LoginLogStatus status, string? message = null);
    }
}