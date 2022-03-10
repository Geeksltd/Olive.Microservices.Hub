using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

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
        }
        public async Task Write(string key, string features) => File.WriteAllText(BaseDirectory + key, features);
        public async Task<string> Read(string key) => File.ReadAllText(BaseDirectory + key);

    }
}
