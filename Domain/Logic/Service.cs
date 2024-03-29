﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3.Model.Internal.MarshallTransformations;
using Newtonsoft.Json;
using Olive;
using Olive.Entities;
using Olive.Entities.Data;
using Olive.Microservices.Hub.Utilities.UrlExtensions;
using Polly.Caching;

namespace Olive.Microservices.Hub
{
    public partial class Service
    {
        public static IEnumerable<Service> All { get; internal set; }
        public string FeaturesJsonPath() => $"/features/services/{Name}.json";
        public string GetBoardSourceUrl() => GetAbsoluteImplementationUrl("olive/board/features/");
        public string GetGlobalSearchUrl()
        {
            if (UseIframe)
                // return GetAbsoluteImplementationUrl("global-search.axd") + "#" + Icon;
                return null;

            return GetAbsoluteImplementationUrl("api/global-search") + "#" + Icon;
        }

        public static object GetAllForJsVariables()
        {
            return All
                .Select(x => new { x.BaseUrl, x.Name })
                .ToList();
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

        public string GetAbsoluteImplementationUrl(string relativeUrl, bool withApi = false) => (withApi ? GetServiceBaseUrlWithApi(BaseUrl, HasApiBackend) : BaseUrl).AppendUrlPath(relativeUrl);
        public static Service GetServiceFromURL(string relativeUrl)
        {
            Service hub = null;
            foreach (var service in All)
            {
                if (service.Name.ToLower() != "hub" && relativeUrl.StartsWith(service.BaseUrl))
                    return service;
                else if (service.Name.ToLower() == "hub" && relativeUrl.StartsWith(service.BaseUrl))
                    hub = service;
            }
            return hub;
        }
        public static Service FindByName(string name)
        {
            var result = All.FirstOrDefault(s => s.Name == name);
            if (result == null) return new Service();
            return result;
        }

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

        string GetServiceBaseUrlWithApi(string baseUrl, bool hasApiBackend)
        {
            if (!hasApiBackend) return baseUrl;
            var domain = Config.Get("Authentication:Cookie:Domain").Trim('/');
            baseUrl = baseUrl.Replace("." + domain, "api." + domain);
            return baseUrl;
        }

        public async Task GetAndSaveFeaturesJson()
        {
            var url = GetAbsoluteImplementationUrl("olive/features", true).AsUri();

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
            var url = GetAbsoluteImplementationUrl("olive/board/sources").AsUri();

            try
            {
                var sources = JsonConvert.DeserializeObject<string[]>(await url.Download(timeOutSeconds: 10));
                if (sources != null)
                {
                    foreach (var source in sources.Select(x => x.ToLower()))
                    {
                        if (BoardSources.BoardComponentSources.ContainsKey(source))
                            BoardSources.BoardComponentSources[source].Add(GetBoardSourceUrl());
                        else BoardSources.BoardComponentSources.Add(source, new List<string> { GetBoardSourceUrl() });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.For(typeof(Service)).Warning(url + " failed:\n" + ex.ToString());
            }
        }

        public async Task GetGlobalSearchSources()
        {
            var url = GetAbsoluteImplementationUrl("/api/global-search?searcher=s");
            if (UseIframe)
                url = GetAbsoluteImplementationUrl("/global-search.axd?searcher=s");

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