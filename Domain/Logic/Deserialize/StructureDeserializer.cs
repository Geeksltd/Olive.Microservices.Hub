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
            if (Feature.All.HasAny() && Context.Current.Environment().EnvironmentName != "Development") LoadBoards();
            Task.Factory.RunSync(ViewModel.BoardComponents.SetBoardSources);
            Task.Factory.RunSync(ViewModel.GlobalSearch.SetSearchSources);

        }

        public static async Task RefreshFeatures() => await Features.RefreshFeatures();

        public static async Task RefreshServiceFeatures() => await Features.RefreshServiceFeatures();

        static void LoadServices()
        {
            Run("LoadServices", () => Service.All == null, () =>
               {

                   var environment = Context.Current.Environment().EnvironmentName.ToLower();
                   if (Context.Current.Environment().EnvironmentName != "Development")
                       Service.All = (from x in ReadXml(GetFromRoot("Services.xml"))
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
                   else
                   {
                       Service.All = Config.Get<List<string>>("Microservice:")
                       .Where(x => x != "Me")
                       .Select(x =>
                       new Service
                       {
                           Name = x,
                           UseIframe = Config.Get("Microservice:" + x + ":Iframe").ToLower() == "true",
                           BaseUrl = Config.Get("Microservice:" + x + ":Url"),
                           Icon = Config.Get("Microservice:" + x + ":Icon"),
                           InjectSingleSignon = Config.Get("Microservice:" + x + ":Sso").ToLower() == "true",
                       }
                       );
                   }
               });
        }

        static void Run(string actionName, Func<bool> condition, Action action)
        {
            if (condition() == false) return;

            var start = LocalTime.Now;

            if (condition() == false) return;

            action();

            Console.WriteLine($"########################### Finished running {actionName} in " + LocalTime.Now.Subtract(start).ToNaturalTime());
        }

        static IEnumerable<XElement> ReadXml(System.IO.FileInfo file)
        {
            var start = LocalTime.Now;
            var result = file.ReadAllText().To<XDocument>().Root.Elements();
            Console.WriteLine($"########################### Finished running ReadXml for {file.Name} in " + LocalTime.Now.Subtract(start).ToNaturalTime());
            return result;
        }

        static FileInfo GetFromRoot(string filename) => AppDomain.CurrentDomain.WebsiteRoot().GetFile(filename);

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

        internal async static Task AddSources(string[] boards, Service service, bool globalsearch)
        {

            if (ViewModel.BoardComponents.BoardComponentSources == null)
                new Dictionary<string, List<string>>()
                {
                { "Person",new List<string>()},
                { "Project",new List<string>()},
                };
            foreach (var board in boards)
                if (!ViewModel.BoardComponents.BoardComponentSources[board].Contains(service.GetBoardSourceUrl()))
                    ViewModel.BoardComponents.BoardComponentSources[board].Append(service.GetBoardSourceUrl());
            if (globalsearch && !ViewModel.GlobalSearch.Sources.Contains(service.GetGlobalSearchUrl()))
            {
                if (!ViewModel.GlobalSearch.Sources.IsEmpty()) ViewModel.GlobalSearch.Sources += ";";
                ViewModel.GlobalSearch.Sources += service.GetGlobalSearchUrl();
            }
        }
        internal static void AddService(Service service)
        {
            if (Service.All.Where(x => x.Name == service.Name).HasAny()) return;
            Service.All.Append(service);
        }
        internal static void AddFeatures(FeatureDefinition[] features)
        {
            var actualFeatures = Features.GetActualFeatures(features);
            foreach (var feature in actualFeatures)
            {
                if (!Feature.All.Where(x => x.GetFullPathSlashSeperated() == feature.GetFullPathSlashSeperated()).HasAny())
                    Feature.All.Append(feature);
            }
        }
        static void LoadBoards()
        {
            Run("LoadBoards", () => Board.All == null, () =>
                {
                    Board.All = ReadXml(GetFromRoot("Boards.xml")).Select(x => new Board(x)).ToList();
                });
        }
        public static async Task<string> GetFeaturesJson() => await Features.Repository.Read("/features/features.json");

    }
}