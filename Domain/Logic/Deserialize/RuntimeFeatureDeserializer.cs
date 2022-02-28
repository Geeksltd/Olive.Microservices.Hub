using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Olive;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;

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
                using (var stream = new Amazon.S3.Transfer.TransferUtility().OpenStream(Config.GetOrThrow("Authentication:HubFeaturesBucket"), "Features"))
                {
                    var task = Task.Run(async () => await stream.ReadAllText());
                    var result = task.WaitAndUnwrapException();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Feature>>(result);
                }
            }
            catch (Exception ex)
            {
                Log.For(typeof(RuntimeFeatureDeserializer)).Error(ex.ToString());
                return Enumerable.Empty<Feature>();
            }

        }
        public async static Task SetFeaturesFromMicroservice()
        {
            var cachedFeatures = new List<Feature>();
            try
            {
                using (var stream = new Amazon.S3.Transfer.TransferUtility().OpenStream(Config.GetOrThrow("Authentication:HubFeaturesBucket"), "Features"))
                    cachedFeatures = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Feature>>(await stream.ReadAllText());

            }
            catch (Exception ex)
            {
                Log.For(typeof(RuntimeFeatureDeserializer)).Error(ex.ToString());
            }
            foreach (var service in Service.All)
            {
                try
                {
                    var url = service.BaseUrl + "/olive/features";
                    var runtimeFeatures = Newtonsoft.Json.JsonConvert.DeserializeObject<RuntimeFeature[]>(new WebClient().DownloadString(url));
                    foreach (var runtimeFeature in runtimeFeatures)
                    {

                        var leaf = runtimeFeature.Features.Split("/").LastOrDefault();
                        var feature = Feature.All.FirstOrDefault(x => x.GetLiniage() == runtimeFeature.Features && x.ImplementationUrl == runtimeFeature.Url && x.Icon == runtimeFeature.Icon);
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
                    }
                }
                catch (Exception ex)
                {
                    Log.For(typeof(RuntimeFeatureDeserializer)).Warning(ex.ToString());
                    foreach (var feature in cachedFeatures.Where(x => x.Service == service))
                        Feature.All.Append(feature);
                }
            }
            using (var stream = new System.IO.MemoryStream(Newtonsoft.Json.JsonConvert.SerializeObject(Feature.All).OrEmpty().ToBytes(System.Text.Encoding.UTF8)))
                await new Amazon.S3.Transfer.TransferUtility().UploadAsync(stream, Config.Get("Features:FeaturesBucketCacheTime"), "Features");
        }

    }
}
