using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Olive;
using Olive.Microservices.Hub;

namespace ViewModel
{
    partial class GlobalSearch
    {
        public string GetSearchSources()
        {
            var globalSearchSources = Config.Bind<GlobalSearchModel1>("GlobalSearch");
            var urls = globalSearchSources.Sources.Where(x => x.Contains("/"))
                .Select(x => new
                {
                    ServiceName = x.Split("/")[0],
                    Url = x.Substring(x.Split("/")[0].Length + 1)
                })
                .Select(x => $"{Service.FindByName(x.ServiceName).GetAbsoluteImplementationUrl(x.Url)}#{Service.FindByName(x.ServiceName).Icon}");
            return urls.ToString(";");
            //var s = ulrs.OrderBy(x => x).ToString(",");

            //var globalSearchConfig = AppDomain.CurrentDomain.WebsiteRoot().GetFile("global-search.json")?.ReadAllText();
            //var globalSearchConfigModel = Newtonsoft.Json.JsonConvert.DeserializeObject<GlobalSearchModel>(globalSearchConfig);
            //if (globalSearchConfigModel == null)
            //    throw new Exception("The global-search.json file was not found!");

            //var olive = globalSearchConfigModel.Olive.Split(',')
            //    .Select(Service.FindByName)
            //    .Select(s => $"{s.GetAbsoluteImplementationUrl("/api/global-search")}#{s.Icon}");

            //var webForms = globalSearchConfigModel.WebForms.Split(',')
            //    .Select(Service.FindByName)
            //    .Select(s => $"{s.GetAbsoluteImplementationUrl(globalSearchConfigModel.Url)}#{s.Icon}");

            //var t = olive.Concat(webForms).OrderBy(x => x).ToString(",");
            //return olive.Concat(webForms).ToString(";");
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