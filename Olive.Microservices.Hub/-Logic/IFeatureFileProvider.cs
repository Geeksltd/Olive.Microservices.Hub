using Domain;
using Olive.Microservices.Hub;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub
{
    public interface IFeatureFileProvider
    {
        Task Save(Service service, string xml);
        Task<string> Load(Service service);
    }
}
