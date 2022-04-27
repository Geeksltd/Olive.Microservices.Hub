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
        internal static Dictionary<string, List<string>> BoardComponentSources;
        public static string GetBoardSources(string type)
        {
            if (BoardComponentSources.ContainsKey(type))
                return BoardComponentSources[type].ToString(";");
            return "";
        }
        public static async Task SetBoardSources()
        {
            new Dictionary<string, List<string>>()
            {
            { "Person",new List<string>()},
            { "Project",new List<string>()},
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