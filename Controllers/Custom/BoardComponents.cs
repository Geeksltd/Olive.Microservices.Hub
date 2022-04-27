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
        internal static Dictionary<string, List<string>> BoardComponentSources = new Dictionary<string, List<string>>();
        public string GetBoardSources(string type)
        {
            if (BoardSources.BoardComponentSources.ContainsKey(type))
                return BoardComponentSources[type].ToString(";");
            return "";
        }
        public static async Task SetBoardSources()
        {

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