using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub
{
    public static class SearchSources
    {
        internal static List<string> Urls = new List<string>();
        public static async Task SetSearchSourceTxt()
        {
            Urls = new List<string>();
            await Task.WhenAll(Service.All.Do(s => s.GetGlobalSearchSources()));
            await Features.Repository.Write("/Search/Sources.txt", Urls.ToString(";"));
            await ViewModel.GlobalSearch.SetSearchSources();
        }
    }
}
