using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Olive;
using Olive.Entities;
using Olive.Entities.Data;
using Olive.Microservices.Hub.Utilities.UrlExtensions;

namespace Olive.Microservices.Hub
{
    public partial class Service
    {
        public static IEnumerable<Service> All { get; internal set; }
        public string FeaturesJsonPath() => $"/features/services/{Name}.json";
        public string GetBoardSourceUrl()
        {
            if (UseIframe)
                return GetAbsoluteImplementationUrl("board-components.axd") + "#" + Icon;
            return GetAbsoluteImplementationUrl("api/board-components") + "#" + Icon;
        }
        public string GetGlobalSearchUrl()
        {
            if (UseIframe)
                return GetAbsoluteImplementationUrl("global-search.axd") + "#" + Icon;
            return GetAbsoluteImplementationUrl("api/global-search") + "#" + Icon;
        }

        public static string ToJson()
        {
            var items = All
                .Select(x => new { x.BaseUrl, x.Name })
                .ToList();

            return JsonConvert.SerializeObject(items);
        }

        protected override async Task OnValidating(EventArgs e)
        {
            await base.OnValidating(e);

            if (!BaseUrl.ToLower().StartsWith("http")) BaseUrl = BaseUrl.WithPrefix("http://");
        }

        public override async Task Validate()
        {
            await base.Validate();

            if (Name.UrlEncode() != Name)
                throw new ValidationException("Name is not in the correct format: " + Name.UrlEncode());
        }

        string GetHubImplementationUrlPrefix() => Name.ToLower().WithWrappers("[", "]");

        public string GetHubImplementationUrl(string relativeUrl)
            => GetHubImplementationUrlPrefix().AppendUrlPath(relativeUrl);

        public string GetAbsoluteImplementationUrl(string relativeUrl) => BaseUrl.AppendUrlPath(relativeUrl);

        public static Service FindByName(string name) => All.FirstOrDefault(s => s.Name == name);

        public class DataProvider : LimitedDataProvider
        {
            public static void Register()
            {
                var config = Context.Current.GetService<IDatabaseProviderConfig>();
                config.RegisterDataProvider(typeof(Service), new DataProvider());
            }

            public override Task<IEntity> Get(object objectID)
            {
                var id = objectID.ToString().To<Guid>();
                return Task.FromResult((IEntity)All.First(x => x.ID == id));
            }
        }

        public async Task GetAndSaveFeaturesJson()
        {
            var url = (BaseUrl + "/olive/features").AsUri();

            try
            {
                var runtimeFeatures = JsonConvert.DeserializeObject<Mvc.Microservices.Feature[]>(await url.Download(timeOutSeconds: 10));
                await Features.Repository.Write(FeaturesJsonPath(), JsonConvert.SerializeObject(runtimeFeatures));
            }
            catch (Exception ex)
            {
                Log.For(typeof(Features)).Warning(url + " failed:\n" + ex.ToString());
            }
        }
        public async Task GetBoardComponentSources()
        {
            var url = (BaseUrl + "/olive/board/sources").AsUri();

            try
            {
                var sources = JsonConvert.DeserializeObject<string[]>(await url.Download(timeOutSeconds: 10));
                foreach (var source in sources)
                {
                    if (BoardSources.BoardComponentSources.ContainsKey(source))
                        BoardSources.BoardComponentSources[source] += ";" + GetBoardSourceUrl();
                    else BoardSources.BoardComponentSources.Add(source, GetBoardSourceUrl());
                }
            }
            catch (Exception ex)
            {
                Log.For(typeof(Service)).Warning(url + " failed:\n" + ex.ToString());
            }
        }
        public async Task GetGlobalSearchSources()
        {
            var url = (BaseUrl + "/api/global-search?searcher=s");
            if (UseIframe)
                url = (BaseUrl + "/global-search.axd?searcher=s");

            try
            {
                var respone = await url.AsUri().Download(timeOutSeconds: 10);
                SearchSources.Urls.Add(GetGlobalSearchUrl());
            }
            catch (Exception ex)
            {
                Log.For(typeof(Service)).Warning(url + " failed:\n" + ex.ToString());
            }
        }
    }
}