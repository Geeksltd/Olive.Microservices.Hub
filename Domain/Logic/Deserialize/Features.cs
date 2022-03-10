using System;
using System.Collections.Generic;
using System.Net;
using Olive;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using Newtonsoft.Json;

namespace Olive.Microservices.Hub
{
    internal static class Features
    {
        static IFeatureRepository Repository;

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]

        class FeatureDefinition : Mvc.Microservices.Feature
        {

            public string ServiceName;
            public FeatureDefinition For(Service service)
            {
                ServiceName = service.Name;
                return this;
            }
        }
        internal static void SetRepository(IFeatureRepository featureRepository) => Repository = featureRepository;
        internal async static Task RefreshServiceFeatures()
        {

            foreach (var service in Service.All)
            {
                var runtimeFeatures = new Mvc.Microservices.Feature[] { };
                try
                {
                    var url = (service.BaseUrl + "/olive/features").AsUri();
                    runtimeFeatures = JsonConvert.DeserializeObject<Mvc.Microservices.Feature[]>(await url.Download());
                }
                catch (Exception ex)
                {
                    Log.For(typeof(Features)).Warning(ex.ToString());
                }
                await Repository.Write($"/features/services/{service.Name}.json", JsonConvert.SerializeObject(runtimeFeatures));
            }
            await RefreshFeatures();
        }
        internal async static Task RefreshFeatures()
        {
            var features = GetXmlFeatures().ToList();
            foreach (var service in Service.All)
            {
                try
                {
                    var batch = JsonConvert.DeserializeObject<FeatureDefinition[]>(await Repository.Read($"/features/services/{service.Name}.json"));
                    batch.Do(x => x.For(service));
                    features.AddRange(batch);
                }
                catch (Exception ex)
                {
                    Log.For(typeof(Features)).Warning(ex.ToString());

                }
            }
            await Repository.Write("/features/features.json", JsonConvert.SerializeObject(features));
            Feature.All = await LoadFeatures();
        }
        internal static async Task<Feature[]> LoadFeatures()
        {
            var json = "";
            try
            {
                json = await Repository.Read("/features/features.json");
            }
            catch (Exception ex)
            {
                Log.For(typeof(Feature)).Error(ex.ToString());
                await RefreshServiceFeatures();
            }
            if (json.HasValue())
            {
                var featureDefinitions = JsonConvert.DeserializeObject<FeatureDefinition[]>(json);
                return GetActualFeatures(featureDefinitions);
            }
            return Feature.All.ToArray();
        }
        static Feature[] GetActualFeatures(FeatureDefinition[] featureDefinitions)
        {
            var features = new List<Feature>();
            foreach (var defenition in featureDefinitions)
            {
                Feature parent = null;
                foreach (var title in defenition.FullPath.Split("/"))
                {
                    var feature = features.FirstOrDefault(x => x.Title.CompareTo(title) == 0);
                    if (feature != null)
                    {
                        parent = feature;
                        continue;
                    }
                    var fullPath = title;
                    if (parent != null) fullPath = parent.GetFullPathSlashSeperated() + "/" + title;
                    var subfeatureDefenition = featureDefinitions.FirstOrDefault(x => x.FullPath.CompareTo(fullPath) == 0);
                    if (defenition.FullPath.ToLower().EndsWith(title.ToLower())) feature = CreateFeature(defenition, parent);
                    else if (subfeatureDefenition != null) feature = CreateFeature(subfeatureDefenition, parent);
                    else
                        feature = CreateFeature(
                            new FeatureDefinition
                            {
                                FullPath = fullPath,
                                Icon = "fas fa-folder",
                                ServiceName = "Hub"
                            }, parent);
                    features.Add(feature);
                    if (parent != null) parent.Children.Append(feature);
                    parent = feature;
                }
            }
            return features.ToArray();
        }
        static Feature CreateFeature(FeatureDefinition featureDefenition, Feature parent)
        {
            var feature = new Feature
            {
                Ref = featureDefenition.Refrance.OrEmpty(),
                Title = featureDefenition.FullPath.Split("/").LastOrDefault(),
                Description = featureDefenition.Description.OrEmpty(),
                ImplementationUrl = featureDefenition.RelativeUrl.OrEmpty(),
                BadgeUrl = featureDefenition.BadgeUrl,
                Icon = featureDefenition.Icon.OrEmpty(),
                ShowOnRight = featureDefenition.ShowOnRight,
                Parent = parent,
                Order = parent?.Children?.Count() * 10 + 10 ?? 10,
                Permissions = featureDefenition.Permissions.OrEmpty().Split(",").Trim().ToArray(),
                NotPermissions = new string[] { },
                Service = Service.FindByName(featureDefenition.ServiceName),
            };
            if (feature.ImplementationUrl.IsEmpty())
                feature.Service = Service.FindByName("Hub");
            feature.LoadUrl = feature.FindLoadUrl().ToLower();
            return feature;
        }
        static IEnumerable<FeatureDefinition> GetXmlFeatures()
        {
            var featuresXml = GetXmlFeatureDefinitions()
                   .SelectMany(x => x.GetAllFeatures())
                   .ExceptNull()
                   .ToList();
            var featureDefenitions = new List<FeatureDefinition>();
            foreach (var featureXml in featuresXml)
            {

                var fullPath = featureXml.GetFullPathSlashSeperated();
                if (featureXml.Title == "add") fullPath = fullPath + "/" + featureXml.Ref;
                if (featureXml.ImplementationUrl.IsEmpty()) continue;
                featureDefenitions.Add(new FeatureDefinition
                {
                    Refrance = featureXml.Ref.OrEmpty(),
                    ServiceName = featureXml.Service.Name,
                    FullPath = fullPath,
                    Icon = featureXml.Icon,
                    Permissions = featureXml.GetPermissionsString(),
                    RelativeUrl = featureXml.ImplementationUrl,
                    BadgeUrl = featureXml.BadgeUrl,
                    Description = featureXml.Description,
                    ShowOnRight = featureXml.ShowOnRight
                });
            }
            return featureDefenitions;
        }

        static XmlFeatureDefinition[] GetXmlFeatureDefinitions()
        {
            var start = LocalTime.Now;
            var stepStart = LocalTime.Now;
            void StartSet() => stepStart = LocalTime.Now;
            var root = new XmlFeatureDefinition(null, new XElement("ROOT"));

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
                     .Select(x => new XmlFeatureDefinition(root, x))
                     .ToArray();

            Log.For(typeof(StructureDeserializer))
                .Info("finished calculating the results in " + LocalTime.Now.Subtract(stepStart).ToNaturalTime());

            Log.For(typeof(StructureDeserializer))
                .Info("finished GetXmlFeatureDefinitions in " + LocalTime.Now.Subtract(start).ToNaturalTime());

            return results;
        }

        private static FileInfo GetFromRoot(string filename) => AppDomain.CurrentDomain.WebsiteRoot().GetFile(filename);

        private static IEnumerable<XElement> ReadXml(System.IO.FileInfo file)
        {
            var start = LocalTime.Now;
            var result = file.ReadAllText().To<XDocument>().Root.Elements();
            Console.WriteLine($"########################### Finished running ReadXml for {file.Name} in " + LocalTime.Now.Subtract(start).ToNaturalTime());
            return result;
        }
    }
}