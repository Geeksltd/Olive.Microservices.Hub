using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Olive;

namespace Domain
{
    public class StructureDeserializer
    {
        public static void Load()
        {
            LoadServices();
            LoadFeatures();
            LoadBoards();
        }

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

        static FeatureDefinition[] GetFeatureDefinitions()
        {
            var start = LocalTime.Now;
            var stepStart = LocalTime.Now;
            void StartSet() => stepStart = LocalTime.Now;
            var root = new FeatureDefinition(null, new XElement("ROOT"));

            Log.For(typeof(StructureDeserializer))
                .Info("finished FeatureDefinition in " + LocalTime.Now.Subtract(stepStart).ToNaturalTime());

            StartSet();
            var files = new[] { "Features.xml", "Features.Widgets.xml" }.Select(f => GetFromRoot(f));

            Log.For(typeof(StructureDeserializer))
                .Info("finished getting files in " + LocalTime.Now.Subtract(stepStart).ToNaturalTime());

            StartSet();

            var results = files
                     .Select(x => ReadXml(x))
                     .SelectMany(x => x)
                     .Select(x => new FeatureDefinition(root, x))
                     .ToArray();

            Log.For(typeof(StructureDeserializer))
                .Info("finished calculating the results in " + LocalTime.Now.Subtract(stepStart).ToNaturalTime());

            Log.For(typeof(StructureDeserializer))
                .Info("finished GetFeatureDefinitions in " + LocalTime.Now.Subtract(start).ToNaturalTime());

            return results;
        }

        static void LoadFeatures()
        {
            Run("LoadFeatures", () => Feature.All == null, () =>
               {
                   Feature.All = GetFeatureDefinitions()
                   .SelectMany(x => x.GetAllFeatures())
                   .ExceptNull()
                   .ToList();

                   foreach (var item in Feature.All)
                       item.Children = Feature.All.Where(x => x.Parent == item);
               });
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