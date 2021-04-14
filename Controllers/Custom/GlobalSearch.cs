using System;
using System.Linq;

using Olive;
using Olive.Microservices.Hub;

namespace ViewModel
{
    partial class GlobalSearch
    {
        public string GetSearchSources()
        {
            var globalSearchConfig = AppDomain.CurrentDomain.WebsiteRoot().GetFile("global-search.json")?.ReadAllText();
            var globalSearchConfigModel = Newtonsoft.Json.JsonConvert.DeserializeObject<GlobalSearchModel>(globalSearchConfig);
            if (globalSearchConfigModel == null)
                throw new Exception("The global-search.json file was not found!");

            var olive = globalSearchConfigModel.Olive.Split(',')
                .Select(Service.FindByName)
                .Select(s => $"{s.GetAbsoluteImplementationUrl("/api/global-search")}#{s.Icon}");

            var webForms = globalSearchConfigModel.WebForms.Split(',')
                .Select(Service.FindByName)
                .Select(s => $"{s.GetAbsoluteImplementationUrl(globalSearchConfigModel.Url)}#{s.Icon}");

            return olive.Concat(webForms).ToString(";");
        }

        public class GlobalSearchModel
        {
            public string Olive { get; set; }
            public string WebForms { get; set; }
            public string Url { get; set; }
        }
    }
}