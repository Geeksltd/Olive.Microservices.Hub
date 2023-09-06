namespace Olive.Microservices.Hub.Domain.Theme.Contracts
{
    using System.Threading.Tasks;

    public interface IThemeValidations
    {
        Task<bool> IsValid(Types.Theme theme);
    }
}