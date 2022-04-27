using System.IO;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub
{
    internal class IOFeatureRepository : IFeatureRepository
    {
        string BaseDirectory;
        internal IOFeatureRepository()
        {
            BaseDirectory = Directory.GetCurrentDirectory();
            Directory.CreateDirectory(BaseDirectory + "/features");
            Directory.CreateDirectory(BaseDirectory + "/features/services");
            Directory.CreateDirectory(BaseDirectory + "/Board");
            Directory.CreateDirectory(BaseDirectory + "/Search");
        }
        public async Task Write(string key, string features) => File.WriteAllText(BaseDirectory + key, features);

        public async Task<string> Read(string key) => File.ReadAllText(BaseDirectory + key);
    }
}