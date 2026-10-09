using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Olive;

namespace Olive.Microservices.Hub
{
    internal static class Features
    {
        public static IFeatureRepository Repository;

        internal static void SetRepository(IFeatureRepository featureRepository) => Repository = featureRepository;

        internal static async Task<List<string>> RefreshServiceFeatures()
        {
            var errors = new ConcurrentList<string>();

            if (Service.All?.Any() == true)
            {
                var throttler = new SemaphoreSlim(initialCount: 10);
                var tasks = Service.All.Select(async service =>
                {
                    await throttler.WaitAsync();
                    try
                    {
                        await service.GetAndSaveFeaturesJson().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        Log.For(typeof(Feature)).Error(e);
                        errors.Add(e.ToString());
                    }
                    finally
                    {
                        throttler.Release();
                    }
                });
                await Task.WhenAll(tasks);
            }

            await RefreshFeatures();

            return errors.ToList();
        }

        internal static async Task RefreshFeatures()
        {
            var features = new List<FeatureDefinition>();

            // Read at the same time, but added in the services' order, as before.
            var batches = await Task.WhenAll(Service.All.Select(async service =>
            {
                try
                {
                    var batch = JsonConvert.DeserializeObject<FeatureDefinition[]>(await Repository.Read(service.FeaturesJsonPath()));
                    batch?.Do(x => x.For(service));
                    return batch;
                }
                catch (Exception ex)
                {
                    Log.For(typeof(Features)).Warning(ex, "Failed to load the features of " + service.Name + ". They are left out.");
                    return null;
                }
            }));

            foreach (var batch in batches)
                if (batch != null) features.AddRange(batch);

            await Repository.Write("/features/features.json", JsonConvert.SerializeObject(features));
            await Load();
        }

        internal static async Task Load()
        {
            var json = "";

            try
            {
                json = await Repository.Read("/features/features.json");
            }
            catch (Exception ex)
            {
                Log.For(typeof(Features)).Warning(ex, "Failed to read features.json. Rebuilding it.");

                // RefreshFeatures rebuilds features.json and loads it, so it is done. Falling through
                // with no json used to rebuild it a second time, from every service over HTTP.
                await RefreshFeatures();
                return;
            }

            if (json.HasValue())
            {
                var featureDefinitions = JsonConvert.DeserializeObject<FeatureDefinition[]>(json);
                Feature.All = GetActualFeatures(featureDefinitions);
            }
            else await RefreshServiceFeatures();

            foreach (var item in Feature.All.OrEmpty()) item.Children = Feature.All.Where(x => x.Parent?.ID == item.ID);
            foreach (var item in Feature.All.OrEmpty().Where(x => x.ImplementationUrl.IsEmpty())) item.Order = item.GetOrder();
            foreach (var item in Feature.All.OrEmpty().Where(x => x.Order == int.MaxValue)) item.Order = 100;

            Feature.All = Feature.All.OrderBy(x => x.Order);
            Feature.DataProvider.Register();
        }

        internal static Feature[] GetActualFeatures(FeatureDefinition[] featureDefinitions)
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
    }
}