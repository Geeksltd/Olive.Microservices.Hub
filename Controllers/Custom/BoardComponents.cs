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
        internal static Dictionary<string, string> BoardComponentSources = new Dictionary<string, string>();
        public string GetBoardSources(string type)
        {
            if (BoardSources.BoardComponentSources.ContainsKey(type))
                return BoardComponentSources[type];
            return "";
        }
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