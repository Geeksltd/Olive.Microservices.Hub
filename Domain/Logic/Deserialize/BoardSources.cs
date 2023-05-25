using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
namespace Olive.Microservices.Hub
{
    public static class BoardSources
    {
        internal static Dictionary<string, List<string>> BoardComponentSources;
        public static async Task SetBoardSourceTxt()
        {
            BoardComponentSources = new Dictionary<string, List<string>>()
            {
            { "person",new List<string>()},
            { "project",new List<string>()},
            };

            await Task.WhenAll(Service.All.Do(s => s.GetBoardComponentSources()));
            foreach (var source in BoardComponentSources)
            {
                BoardComponentSources[source.Key] = source.Value.OrderBy(x=>Service.GetServiceFromURL(x).Name).ToList();
            }
            await Features.Repository.Write("/Board/Sources.txt", JsonConvert.SerializeObject(BoardComponentSources));
            await ViewModel.BoardComponents.SetBoardSources();
        }
    }
}