using System.IO;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub
{
    internal class IOBoardsRepository : IBoardsRepository
    {
        string BaseDirectory;
        internal IOBoardsRepository() => BaseDirectory = Directory.GetCurrentDirectory();
        public async Task Write(string key, string boards) => File.WriteAllText(BaseDirectory + key, boards);

        public async Task<string> Read(string key) => File.ReadAllText(BaseDirectory + key);
    }
}