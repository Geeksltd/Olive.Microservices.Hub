using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub
{
    internal interface IFeatureRepository
    {
        Task Write(string key, string json);
        Task<string> Read(string key);

    }
}
