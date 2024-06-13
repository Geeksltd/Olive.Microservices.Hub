using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Olive;
using Olive.Microservices.Hub;

namespace ViewModel
{
    partial class BoardComponents
    {
        internal static Dictionary<string, List<string>> BoardComponentSources;
        public string GetBoardSources(string type)
        {
            if (BoardComponentSources.ContainsKey(type.ToLower()))
            {
                var items = BoardComponentSources[type.ToLower()];
                if (items != null)
                {
                    return items.Distinct().ToString(";");
                }
            }

            return "";
        }

        public static async Task SetBoardSources()
        {
            BoardComponentSources = new Dictionary<string, List<string>>()
            {
            { "person",new List<string>()},
            { "project",new List<string>()},
            };

            try
            {
                BoardComponentSources = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(await Features.Repository.Read("/Board/Sources.txt"));
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