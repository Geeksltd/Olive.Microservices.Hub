using System;
using System.Linq;
using Newtonsoft.Json;

namespace Olive.Microservices.Hub
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class FeatureDefinition : Mvc.Microservices.Feature
    {
        public string ServiceName;
        public FeatureDefinition For(Service service)
        {
            ServiceName = service.Name;
            return this;
        }

        public Feature CreateFeature(Feature parent)
        {
            var feature = new Feature
            {
                ID = HubEncoder.ConvertStringToGuid(FullPath),
                Ref = Refrance.OrEmpty(),
                Title = FullPath.Split("/").LastOrDefault(),
                Description = Description.OrEmpty(),
                ImplementationUrl = RelativeUrl.OrEmpty(),
                BadgeUrl = BadgeUrl,
                Icon = Icon.OrEmpty(),
                ShowOnRight = ShowOnRight,
                Parent = parent,
                Order = Order ?? int.MaxValue,
                NoNav = NoNav,
                Permissions = Permissions.OrEmpty().Split(",").Trim().Where(x => !x.StartsWith("!")).ToArray(),
                NotPermissions = Permissions.OrEmpty().Split(",").Trim().Where(x => x.StartsWith("!")).Select(x => x.TrimStart("!")).ToArray(),
                Service = Service.FindByName(ServiceName),
                UseIframe = Iframe
            };

            // Add prefixes:
            feature.Permissions = feature.Permissions.Where(x => x.Lacks(":")).Select(x => $"{ServiceName}:{x}").Concat(feature.Permissions).ToArray();

            if (feature.ImplementationUrl.IsEmpty())
                feature.Service = Service.FindByName("Hub");

            feature.LoadUrl = feature.FindLoadUrl().ToLower();
            return feature;
        }
    }
}