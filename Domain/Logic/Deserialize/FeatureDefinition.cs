using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Olive.Microservices.Hub
{

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    class FeatureDefinition : Mvc.Microservices.Feature
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
                Ref = Refrance.OrEmpty(),
                Title = FullPath.Split("/").LastOrDefault(),
                Description = Description.OrEmpty(),
                ImplementationUrl = RelativeUrl.OrEmpty(),
                BadgeUrl = BadgeUrl,
                Icon = Icon.OrEmpty(),
                ShowOnRight = ShowOnRight,
                Parent = parent,
                Order = parent?.Children?.Count() * 10 + 10 ?? 10,
                Permissions = Permissions.OrEmpty().Split(",").Trim().ToArray(),
                NotPermissions = new string[] { },
                Service = Service.FindByName(ServiceName),
            };
            if (feature.ImplementationUrl.IsEmpty())
                feature.Service = Service.FindByName("Hub");
            feature.LoadUrl = feature.FindLoadUrl().ToLower();
            return feature;
        }
    }
}
