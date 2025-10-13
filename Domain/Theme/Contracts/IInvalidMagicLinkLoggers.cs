using System;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub.Domain.Theme.Contracts
{
    public interface IInvalidMagicLinkLoggers
    {
        Task Log(Theme.Types.Theme currentTheme, string? email, string token, DateTime? createOn);
    }
}