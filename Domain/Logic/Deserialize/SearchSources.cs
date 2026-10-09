using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub
{
    public static class SearchSources
    {
        internal static List<string> Urls = new List<string>();
        public static async Task SetSearchSourceTxt()
        {
            Urls = new List<string>();
            await Task.WhenAll(Service.All.Select(s => s.GetGlobalSearchSources()));

            // The services answer in any order. Kept in the services' order, as when they were asked one by one.
            var order = Service.All.Select(s => s.GetGlobalSearchUrl()).ToList();
            Urls = Urls.OrderBy(order.IndexOf).ToList();
            await Features.Repository.Write("/Search/Sources.txt", Urls.Trim().ToString(";"));
            await ViewModel.GlobalSearch.SetSearchSources();
        }
    }
}
