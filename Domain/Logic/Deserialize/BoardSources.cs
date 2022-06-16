using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
            foreach (var service in Service.All) await service.GetBoardComponentSources();
            await Features.Repository.Write("/Board/Sources.txt", JsonConvert.SerializeObject(BoardComponentSources));
            await ViewModel.BoardComponents.SetBoardSources();
        }

    }
}
