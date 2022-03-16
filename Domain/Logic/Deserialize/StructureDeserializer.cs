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
            if (Feature.All.HasAny()) LoadBoards();
        }

        public static async Task RefreshFeatures() => await Features.RefreshFeatures();

        public static async Task RefreshServiceFeatures() => await Features.RefreshServiceFeatures();

        static void LoadServices()
        {
            Run("LoadServices", () => Service.All == null, () =>
               {
                   var environment = Context.Current.Environment().EnvironmentName.ToLower();

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


        static void LoadBoards()
        {
            Run("LoadBoards", () => Board.All == null, () =>
                {
                    Board.All = ReadXml(GetFromRoot("Boards.xml")).Select(x => new Board(x)).ToList();
                });
        }
    }
}