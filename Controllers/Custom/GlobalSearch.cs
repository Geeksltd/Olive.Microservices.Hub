using System.Linq;

using Olive;
using Olive.Microservices.Hub;

namespace ViewModel
{
    partial class GlobalSearch
    {
        public string GetSearchSources()
        {
            var olive = "Hub,Tasks,People,Projects".Split(',')
                .Select(Service.FindByName)
                .Select(s => $"{s.GetAbsoluteImplementationUrl("/api/global-search")}#{s.Icon}");

            var webForms = "Accounting,HR,CRM,CaseStudies,Training".Split(',')
                .Select(Service.FindByName)
                .Select(s => $"{s.GetAbsoluteImplementationUrl("/global-search.axd")}#{s.Icon}");

            return olive.Concat(webForms).ToString(";");
        }
    }
}