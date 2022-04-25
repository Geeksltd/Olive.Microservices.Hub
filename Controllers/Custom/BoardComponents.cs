using System.Linq;
using Olive;
using Olive.Microservices.Hub;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ViewModel
{
    partial class BoardComponents
    {
        internal static Dictionary<string, string> BoardComponentSources;
        public string GetBoardSources(string type) => BoardComponentSources[type];
        public static async Task SetBoardSources()
        {
            foreach (var service in Service.All)
            {
                var sources = await service.GetBoardComponentSources();
                foreach (var source in sources)
                {
                    if (BoardComponentSources.ContainsKey(source))
                        BoardComponentSources[source] += ";" + service.GetBoardSourceUrl();
                    else BoardComponentSources.Add(source, service.GetBoardSourceUrl());

                }
            }
        }
        public class GlobalSearchModel1
        {
            public string[] Project { get; set; }
            public string[] Person { get; set; }
        }
    }
}