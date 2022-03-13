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
        public static IFeatureRepository Repository;

        internal static void SetRepository(IFeatureRepository featureRepository) => Repository = featureRepository;
        internal async static Task RefreshServiceFeatures()
        {
            await Task.WhenAll(Service.All.Do(s => s.GetAndSaveFeaturesJson()));
            await RefreshFeatures();
        }
        internal async static Task RefreshFeatures()
        {
            var features = FromLegacyXml().ToList();
            foreach (var service in Service.All)
            {
                try
                {
                    var batch = JsonConvert.DeserializeObject<FeatureDefinition[]>(await Repository.Read(service.FeaturesJsonPath()));
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
                await RefreshFeatures();
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
            var featuresDictionary = new Dictionary<string, List<Feature>>();
            foreach (var definition in featureDefinitions)
            {
                Feature parent = null;
                var path = "";
                var titles = definition.FullPath.Split('/');
                foreach (var title in titles)
                {
                    if (path.HasValue()) path += "/";
                    path += title;
                    var feature = featuresDictionary.GetOrDefault(path)?.FirstOrDefault();
                    if (definition.FullPath == path) feature = definition.CreateFeature(parent);
                    else if (feature != null && feature.ImplementationUrl.IsEmpty())
                    {
                        parent = feature;
                        continue;
                    }
                    else feature = new FeatureDefinition { FullPath = path, Icon = "fas fa-folder", ServiceName = "Hub" }.CreateFeature(parent);
                    var features = featuresDictionary.GetOrDefault(path);
                    if (features == null)
                    {
                        features = new List<Feature>();
                        featuresDictionary.Add(path, features);
                    }
                    features.Add(feature);

                    if (parent != null) parent.Children.Append(feature);
                    parent = feature;
                }
            }
            return featuresDictionary.Values.SelectMany(x => x).ToArray();
        }

        static IEnumerable<FeatureDefinition> FromLegacyXml()
        {
            var featuresXml = GetXmlFeatureDefinitions()
                   .SelectMany(x => x.GetAllFeatures())
                   .ExceptNull()
                   .ToList();
            foreach (var item in featuresXml)
                item.Children = featuresXml.Where(x => x.Parent?.ID == item.ID);
            var featureDefenitions = new List<FeatureDefinition>();
            foreach (var featureXml in featuresXml)
            {
                if (featureXml.HasSimilarChild(featureXml.ImplementationUrl)) continue;
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