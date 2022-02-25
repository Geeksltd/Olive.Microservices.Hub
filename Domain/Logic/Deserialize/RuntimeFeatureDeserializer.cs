using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Olive;
using System.Linq;

namespace Olive.Microservices.Hub
{
    public class RuntimeFeature
    {
        public string Features;
        public string Icon;
        public string Url;
        public string Permissions;
        public string Desc;
    }
    internal static class RuntimeFeatureDeserializer
    {
        static MemoryCache CachedFeatures = new MemoryCache(new MemoryCacheOptions());
        public static IEnumerable<Feature> GetCachedFeatures()
        {

            try
            {
                return (IEnumerable<Feature>)CachedFeatures.Get(Config.Get("Features:FeaturesBucketCacheName", "Features"));

            }
            catch (Exception ex)
            {
                Log.For(typeof(RuntimeFeatureDeserializer)).Error(ex.ToString());
                return Enumerable.Empty<Feature>();
            }

        }
        public static void SetFeaturesFromMicroservice()
        {
            foreach (var service in Service.All)
            {
                try
                {
                    var url = service.BaseUrl + "/olive/features";
                    var runtimeFeatures = Newtonsoft.Json.JsonConvert.DeserializeObject<RuntimeFeature[]>(new WebClient().DownloadString(url));
                    var features = new List<Feature>();
                    foreach (var runtimeFeature in runtimeFeatures)
                    {
                        var leaf = runtimeFeature.Features.Split("/").LastOrDefault();
                        var feature = Feature.All.FirstOrDefault(x => x.GetLiniage() == runtimeFeature.Features);
                        if (feature == null)
                        {
                            Feature parent = Feature.All.FirstOrDefault(x => x.GetLiniage() == runtimeFeature.Features.Remove("/" + leaf)); ;
                            feature = new Feature
                            {
                                Title = leaf,
                                Description = runtimeFeature.Desc,
                                ImplementationUrl = runtimeFeature.Url,
                                Icon = runtimeFeature.Icon,
                                ShowOnRight = false,
                                Parent = parent,
                                Order = parent.Children.Select(x => x.Order).Max() + 10,
                                Permissions = runtimeFeature.Permissions.Split(",").Trim().ToArray(),
                                Service = service,
                            };
                            if (parent != null) parent.Children.Append(feature);
                            Feature.All.Append(feature);
                        }
                        features.Add(feature);
                    }
                }
                catch (Exception ex)
                {
                    Log.For(typeof(RuntimeFeatureDeserializer)).Warning(ex.ToString());
                }
            }
            CachedFeatures.Set(Config.Get("Features:FeaturesBucketCacheName", "Features"), Feature.All, Config.Get("Features:FeaturesBucketCacheTime", 30).Minutes());
        }

    }
}
