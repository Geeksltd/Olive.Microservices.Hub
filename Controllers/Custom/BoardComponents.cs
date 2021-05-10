using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Olive;
using Olive.Microservices.Hub;
using Olive.Microservices.Hub.BoardComponent;

namespace ViewModel
{
    partial class BoardComponents
    {
        public string GetBoardSources(string type)
        {
            var boardSources = Config.Bind<GlobalSearchModel1>("BoardComponents:Sources");
            var sources = type == "Project" ? boardSources.Project : boardSources.Person;
            var urls = sources.Where(x => x.Contains("/"))
                .Select(x => new
                {
                    ServiceName = x.Split("/")[0],
                    Url = x.Substring(x.Split("/")[0].Length + 1)
                })
                .Select(x => $"{Service.FindByName(x.ServiceName).GetAbsoluteImplementationUrl(x.Url)}#{Service.FindByName(x.ServiceName).Icon}");
            return urls.ToString(";");
        }
        public class GlobalSearchModel1
        {
            public string[] Project { get; set; }
            public string[] Person { get; set; }
        }
    }
}