using System;
using System.Linq;
using Microsoft.TeamFoundation.Common;
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
                ID=HashtoGuid(StringSha256Hash(FullPath)).To<Guid>(),
                Ref = Refrance.OrEmpty(),
                Title = FullPath.Split("/").LastOrDefault(),
                Description = Description.OrEmpty(),
                ImplementationUrl = RelativeUrl.OrEmpty(),
                BadgeUrl = BadgeUrl,
                Icon = Icon.OrEmpty(),
                ShowOnRight = ShowOnRight,
                Parent = parent,
                Order = Order ?? int.MaxValue,
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



         static string StringSha256Hash(string text) =>
        text.IsNullOrEmpty() ? string.Empty : BitConverter.ToString(new System.Security.Cryptography.SHA1Managed().ComputeHash(System.Text.Encoding.UTF8.GetBytes(text))).Replace("-", string.Empty);


        static string HashtoGuid(string input) => input.IsNullOrEmpty() ? "" : $"{input.Substring(0, 8)}-{input.Substring(8, 4)}-{input.Substring(12, 4)}-{input.Substring(16, 4)}-{input.Substring(20, 12)}";
    }
}
