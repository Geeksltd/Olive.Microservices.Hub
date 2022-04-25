using System.Linq;
using System.Threading.Tasks;
using Olive;
using Olive.Microservices.Hub;
using System.Collections.Generic;

namespace ViewModel
{
    partial class GlobalSearch
    {
        internal static string SearchSources;
        public string GetSearchSources() => SearchSources;
        public static async Task SetSearchSources()
        {
            var urls = new List<string>();
            foreach (var service in Service.All)
            {
                var searchable = await service.GetGlobalSearchSources();
                if (searchable) urls.Add(service.GetGlobalSearchUrl());
            }
            SearchSources = urls.ToString(";");
            // var s = ulrs.OrderBy(x => x).ToString(",");

            // var globalSearchConfig = AppDomain.CurrentDomain.WebsiteRoot().GetFile("global-search.json")?.ReadAllText();
            // var globalSearchConfigModel = Newtonsoft.Json.JsonConvert.DeserializeObject<GlobalSearchModel>(globalSearchConfig);
            // if (globalSearchConfigModel == null)
            //    throw new Exception("The global-search.json file was not found!");

            // var olive = globalSearchConfigModel.Olive.Split(',')
            //    .Select(Service.FindByName)
            //    .Select(s => $"{s.GetAbsoluteImplementationUrl("/api/global-search")}#{s.Icon}");

            // var webForms = globalSearchConfigModel.WebForms.Split(',')
            //    .Select(Service.FindByName)
            //    .Select(s => $"{s.GetAbsoluteImplementationUrl(globalSearchConfigModel.Url)}#{s.Icon}");

            // var t = olive.Concat(webForms).OrderBy(x => x).ToString(",");
            // return olive.Concat(webForms).ToString(";");
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