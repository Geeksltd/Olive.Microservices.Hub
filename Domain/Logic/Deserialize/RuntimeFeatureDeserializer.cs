using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Olive;
using System.Linq;

namespace Olive.Microservices.Hub
{
    internal static class RuntimeFeatureDeserializer
    {
        static MemoryCache CachedFeatures = new MemoryCache(new MemoryCacheOptions());
        public class RuntimeFeature
        {
            public string[] Features;
            public string Icon;
            public string Url;
            public string Permissions;
            public string Desc;
        }
        public static void SetRuntimeFeatures()
        {
            foreach (var service in Service.All)
            {
                SetRuntimeFeaturesCache(service);
            }
        }
        public static void SetRuntimeFeaturesCache(Service service)
        {
            try
            {
                var features = GetAndSetFeaturesFromMicroservice(service);
                if (features != null)
                    CachedFeatures.Set(service.Name, features, Config.Get("Features:FeaturesBucketCache", 30).Minutes());
            }
            catch (Exception ex)
            {
                try
                {
                    var features = (IEnumerable<Feature>)CachedFeatures.Get(service.Name);
                    foreach (var feature in features.ToList())
                    {
                        Feature.All.Append(feature);
                        if (feature.Parent != null) feature.Parent.Children.Append(feature);
                    }
                    Log.For(typeof(RuntimeFeatureDeserializer)).Warning(ex.ToString());
                }
                catch (Exception ex2)
                {
                    Log.For(typeof(RuntimeFeatureDeserializer)).Warning(ex2.ToString());
                }
                Log.For(typeof(RuntimeFeatureDeserializer)).Warning(ex.ToString());

            }
        }
        public static IEnumerable<Feature> GetAndSetFeaturesFromMicroservice(Service service)
        {
            var url = service.BaseUrl + "/olive/features";
            var runtimeFeatures = Newtonsoft.Json.JsonConvert.DeserializeObject<RuntimeFeature[]>(new WebClient().DownloadString(url));
            var features = new List<Feature>();
            foreach (var runtimeFeature in runtimeFeatures)
            {
                var leaf = runtimeFeature.Features[runtimeFeature.Features.Length - 1];
                Feature parent = null;
                foreach (var name in runtimeFeature.Features)
                {

                    if (name == leaf)
                    {
                        var feature = new Feature
                        {
                            Title = name,
                            Description = runtimeFeature.Desc,
                            ImplementationUrl = runtimeFeature.Url,
                            Icon = runtimeFeature.Icon,
                            ShowOnRight = false,
                            Parent = parent,
                            Order = parent.Children.Select(x => x.Order).Max() + 10,
                            Permissions = runtimeFeature.Permissions.Split(",").Trim().ToArray(),
                            Service = service,
                        };
                        Feature.All.Append(feature);
                        if (parent != null) parent.Children.Append(feature);
                        features.Add(feature);
                    }

                    else parent = FindWithParent(parent, name);

                }
            }
            return features;
        }
        public static Feature FindWithParent(Feature parent, string Name)
        {
            if (parent == null) return Feature.All.FirstOrDefault(x => x.Title == Name);
            return parent.Children.FirstOrDefault(x => x.Title == Name);
        }
    }
}
