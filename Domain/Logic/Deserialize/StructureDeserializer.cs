using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Olive;

namespace Olive.Microservices.Hub
{
    public class StructureDeserializer
    {
        static DateTime LastLoad = LocalTime.UtcNow;
        public static void Load()
        {
            LoadServices();
            LoadFeatures();
            if (Context.Current.Environment().EnvironmentName != "Development")
            {
                Task.Factory.RunSync(ViewModel.BoardComponents.SetBoardSources);
                Task.Factory.RunSync(ViewModel.GlobalSearch.SetSearchSources);
            }      

        }

        public static async Task RefreshFeatures() => await Features.RefreshFeatures();

        public static async Task RefreshServiceFeatures() => await Features.RefreshServiceFeatures();

        static void LoadServices()
        {
            Run("LoadServices", () => Service.All == null, () =>
               {
                   try
                   {
                       Task.Factory.RunSync(SetServicesFromXml);
                   }
                   catch
                   {
                       Service.All = Config.GetSubsection("Microservice", true).Select(x => x.Key)
                         .Where(x => !x.Contains("Me") && !x.Contains(":"))
                         .Select(x =>
                         new Service
                         {
                             Name = x,
                             Icon = Config.Get("Microservice:" + x + ":Icon"),
                             UseIframe = Config.Get("Microservice:" + x + ":Iframe").ToLower() == "true",
                             BaseUrl = Config.Get("Microservice:" + x + ":Url"),
                             InjectSingleSignon = Config.Get("Microservice:" + x + ":Sso").ToLower() == "true",
                         });
                   }
               });
        }

        static async Task SetServicesFromXml()
        {
            var serviceXml = await Features.Repository.Read("/Services.xml");
            var environment = Context.Current.Environment().EnvironmentName.ToLower();

            Service.All = (from x in serviceXml.To<XDocument>().Root?.Elements()
                           let envDomain = x.Parent.GetValue<string>("@" + environment)
                           let url = x.GetValue<string>("@" + environment) ?? x.GetValue<string>("@url")
                           select new Service
                           {
                               Name = x.GetCleanName(),
                               UseIframe = x.GetValue<bool?>("@iframe") ?? false,
                               BaseUrl = (url.StartsWith("http") ? url : $"https://{url}.{envDomain}").ToLower(),
                               Icon = x.GetValue<string>("@icon"),
                               InjectSingleSignon = x.GetValue<bool?>("@sso") ?? false

                           }).ToList();
        }

        static void Run(string actionName, Func<bool> condition, Action action)
        {
            if (condition() == false) return;

            var start = LocalTime.Now;

            if (condition() == false) return;

            action();

            Console.WriteLine($"########################### Finished running {actionName} in " + LocalTime.Now.Subtract(start).ToNaturalTime());
        }
        static void LoadFeatures()
        {
            Run("LoadFeatures", () => Feature.All == null, () =>
            {
                Task.Factory.RunSync(Features.Load);
                LastLoad = LocalTime.UtcNow;
            });
        }

        internal static RequestDelegate ReloadFeatures(RequestDelegate next)
        {
            return async ctx =>
            {
                if (LocalTime.UtcNow - LastLoad > 2.Minutes())
                {
                    LastLoad = LocalTime.UtcNow;
                    await Features.Load();
                }

                await next(ctx);
            };
        }

        internal static RequestDelegate ReloadSources(RequestDelegate next)
        {
            return async ctx =>
            {
                if (LocalTime.UtcNow - LastLoad > 2.Minutes())
                {
                    LastLoad = LocalTime.UtcNow;
                    await ViewModel.BoardComponents.SetBoardSources();
                    await ViewModel.GlobalSearch.SetSearchSources();
                }

                await next(ctx);
            };
        }

        internal static void AddSources(string[] boards, Service service, bool globalsearch)
        {
            if (ViewModel.BoardComponents.BoardComponentSources == null)
                ViewModel.BoardComponents.BoardComponentSources = new Dictionary<string, List<string>>()
                {
                { "person",new List<string>()},
                { "project",new List<string>()},
                };
            foreach (var board in boards.Select(x => x.ToLower()))
            {
                if (!ViewModel.BoardComponents.BoardComponentSources.ContainsKey(board))
                    ViewModel.BoardComponents.BoardComponentSources.Add(board, new List<string> { service.GetBoardSourceUrl() });
                else if (!ViewModel.BoardComponents.BoardComponentSources[board].Contains(service.GetBoardSourceUrl()))
                    ViewModel.BoardComponents.BoardComponentSources[board].Append(service.GetBoardSourceUrl());
            }
            if (globalsearch && !ViewModel.GlobalSearch.Sources.Contains(service.GetGlobalSearchUrl()))
            {
                if (ViewModel.GlobalSearch.Sources.HasValue()) ViewModel.GlobalSearch.Sources += ";";
                ViewModel.GlobalSearch.Sources += service.GetGlobalSearchUrl();
            }
        }

        internal static void AddService(Service service)
        {
            if (Service.All.Where(x => x.Name == service.Name).HasAny()) return;
            var services = new List<Service>() { service };
            services.AddRange(Service.All);
            Service.All = services.ToList();
        }

        internal static void AddFeatures(FeatureDefinition[] features)
        {
            if (!features.HasAny()) return;
            var actualFeatures = Features.GetActualFeatures(features).ToList();

            if (Feature.All.HasAny())
            {
                var featuresFullPath = Feature.All.Select(y => y.GetFullPathSlashSeperated());
                actualFeatures = actualFeatures.Where(x => !x.GetFullPathSlashSeperated().IsAnyOf(featuresFullPath)).ToList();
                actualFeatures.AddRange(Feature.All);
            }

            Feature.All = actualFeatures;

            foreach (var item in Feature.All.OrEmpty())
            {
                item.Children = Feature.All.Where(x => x.Parent?.GetFullPathSlashSeperated() == item.GetFullPathSlashSeperated());
                item.Children.Do(x => x.Parent = item);
            }

            foreach (var item in Feature.All.OrEmpty().Where(x => x.ImplementationUrl.IsEmpty())) item.Order = item.GetOrder();
            foreach (var item in Feature.All.OrEmpty().Where(x => x.Order == int.MaxValue)) item.Order = 100;
            Feature.All = Feature.All.OrderBy(x => x.Order);
        }
        public static async Task<string> GetFeaturesJson() => await Features.Repository.Read("/features/features.json");

    }
}