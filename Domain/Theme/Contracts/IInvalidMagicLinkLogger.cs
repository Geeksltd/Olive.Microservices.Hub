using System;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub.Domain.Theme.Contracts
{
    public interface IInvalidMagicLinkLogger
    {
        string Name { get; }
        Task Log(string[] to, string? email, string token, DateTime? createOn);
    }
}