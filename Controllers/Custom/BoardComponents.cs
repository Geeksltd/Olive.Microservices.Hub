using System.Linq;
using Olive;
using Olive.Microservices.Hub;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;

namespace ViewModel
{
    partial class BoardComponents
    {
        internal static Dictionary<string, string> BoardComponentSources;
        public string GetBoardSources(string type) => BoardComponentSources[type];
        public static async Task SetBoardSources()
        {

            try
            {
                BoardComponentSources = JsonConvert.DeserializeObject<Dictionary<string, string>>(await Features.Repository.Read("/Board/Sources.txt"));
            }
            catch (Exception ex)
            {
                Log.For(typeof(GlobalSearch)).Warning(" failed to read board sources:\n" + ex.ToString());
                await BoardSources.SetBoardSourceTxt();
            }
        }
        public class GlobalSearchModel1
        {
            public string[] Project { get; set; }
            public string[] Person { get; set; }
        }
    }
}