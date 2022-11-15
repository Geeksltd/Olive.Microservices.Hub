using System.Threading.Tasks;

namespace Olive.Microservices.Hub
{
    internal interface IBoardsRepository
    {
        Task Write(string key, string json);
        Task<string> Read(string key);

    }
}
