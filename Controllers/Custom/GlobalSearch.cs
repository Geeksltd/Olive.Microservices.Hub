using System;
using System.Threading.Tasks;
using Olive;
using Olive.Microservices.Hub;

namespace ViewModel
{
    partial class GlobalSearch
    {
        internal static string Sources = "";
        public string GetSearchSources() => Sources;

        public static async Task SetSearchSources()
        {
            try
            {
                Sources = await Features.Repository.Read("/Search/Sources.txt");
            }
            catch (Exception ex)
            {
                Log.For(typeof(GlobalSearch)).Warning(" failed to read search sources:\n" + ex.ToString());
                await SearchSources.SetSearchSourceTxt();
            }
        }
        public class GlobalSearchModel
        {
            public string Olive { get; set; }
            public string WebForms { get; set; }
            public string Url { get; set; }
        }
        public class GlobalSearchModel1
        {
            public string[] Sources { get; set; }
        }
    }
}