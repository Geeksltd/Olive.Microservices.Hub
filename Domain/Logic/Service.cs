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
        public string GetBoardSourceUrl() => GetAbsoluteImplementationUrl("olive/board/features") + "#" + Icon;
        public string GetGlobalSearchUrl() => GetAbsoluteImplementationUrl("api/global-search") + "#" + Icon;

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
        public async Task<string[]> GetBoardComponentSources()
        {
            var url = (BaseUrl + "/olive/board/sources").AsUri();

            try
            {
                return JsonConvert.DeserializeObject<string[]>(await url.Download(timeOutSeconds: 10));
            }
            catch (Exception ex)
            {
                Log.For(typeof(Features)).Warning(url + " failed:\n" + ex.ToString());
            }
            return null;
        }
        public async Task<bool> GetGlobalSearchSources()
        {
            var url = (BaseUrl + "/olive/search/sources").AsUri();

            try
            {
                var respone = await url.Download(timeOutSeconds: 10);
                if (respone.HasValue())
                    return true;
            }
            catch (Exception ex)
            {
                Log.For(typeof(Features)).Warning(url + " failed:\n" + ex.ToString());
            }
            return false;
        }
    }
}