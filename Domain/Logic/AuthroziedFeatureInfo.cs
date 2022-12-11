namespace Olive.Microservices.Hub
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Newtonsoft.Json;
    using Olive;

    partial class AuthroziedFeatureInfo
    {
        public static XElement RenderMenu(Feature currentFeature)
        {
            var items = FeatureSecurityFilter.GetAuthorizedFeatures(Context.Current.User());
            items = AddEverythingItem(items);
            return RenderMenu(currentFeature, items);
        }

        static AuthroziedFeatureInfo[] AddEverythingItem(AuthroziedFeatureInfo[] items)
        {
            var everything = new AuthroziedFeatureInfo
            {
                Feature = new Feature
                {
                    Title = "Everything",
                    LoadUrl = "/[hub]/everything",
                    Icon = "fas fa-th"
                }
            };

            return items.Prepend(everything).ToArray();
        }

        public static async Task<XElement> RenderMenuJson()
        {
            var jsonMenu = await RenderJsonMenu();

            return new XElement("input",
                   new XAttribute("id", "topMenu"),
                   new XAttribute("type", "hidden"),
                   new XAttribute("value", jsonMenu));
        }

        public static async Task<string> RenderFullMenu()
        {
            ColourPalette.Reset();
            var items = FeatureSecurityFilter.GetAuthorizedFeatures(Context.Current.User());
            var menuItems = await GetAllMenuItems(items);

            var sorted = menuItems
                .OrderBy(x => x.Children == null ? 0 : x.Children.Sum(c => c.Children == null ? 1 : c.Children.Count() + 1))
                .ToList();

            var container = new XElement("div");

            sorted.ForEach(item =>
            {
                var div = new XElement("div");
                div.Add(new XAttribute("class", "full-menu-item"));

                if (item.Children != null && item.Children.Any())
                {
                    var ul = new XElement("ul");

                    item.Children.ToList().ForEach(subItem =>
                    {
                        var li = new XElement("li", CreateSubLink(subItem, ColourPalette.GetColourCode()));
                        ul.Add(li);
                        AddChildItems(subItem, ColourPalette.GetColourCode(), li);
                    });

                    div.Add(ul);

                    div.Add(new XElement("h3", new XAttribute("class", "full-menu-text"),
                        new XElement("a", item.Title, new XAttribute("href", item.LoadUrl))));
                }
                else
                {
                    div.Add(new XElement("ul", new XElement("li", CreateSubLink(item, ColourPalette.GetColourCode()))));
                }

                container.Add(div);
            });

            return container.ToString().TrimStart("<div>").TrimEnd("</div>");
        }

        static void AddChildItems(JsonMenu subItem, string color, XElement parent)
        {
            if (subItem.Children == null || subItem.Children.None()) return;
            var ul2 = new XElement("ul");

            subItem.Children
                .ToList()
                .ForEach(subItem2 =>
            {
                var li2 = new XElement("li");
                var subLink2 = CreateSubLink(subItem2, color);

                li2.Add(subLink2);
                ul2.Add(li2);
            });

            parent.Add(ul2);
        }

        static XElement CreateSubLink(JsonMenu subItem, string color)
        {
            var subLink = new XElement("a", subItem.Title,
               new XAttribute("id", subItem.LogicalPath),
               new XAttribute("href", subItem.LoadUrl));

            var subIcon = new XElement("i", string.Empty);
            subIcon.Add(new XAttribute("style", $"color:{color};"));

            if (subItem.Icon != null)
                subIcon.Add(new XAttribute("class", subItem.Icon));

            subLink.Add(subIcon);
            return subLink;
        }

        static XElement RenderMenu(Feature currentFeature, IEnumerable<AuthroziedFeatureInfo> items)
        {
            if (items.None()) return null;

            var rootMEnuId = Guid.NewGuid();

            if (currentFeature != null)
            {
                rootMEnuId = currentFeature.ID;
            }

            var ul = new XElement("ul", new XAttribute("class", "nav navbar-nav dropped-submenu"), new XAttribute("id", rootMEnuId));
            //items= items.OrderBy(x=>x.Feature.Title).ToList();
            foreach (var item in items)
            {
                var feature = item.Feature;

                var li = new XElement("li",
                    new XAttribute("id", feature.ID),
                    new XAttribute("class",
                    string.Format("feature-menu-item{0}{1}",
                    " active".OnlyWhen(feature == currentFeature),
                    " d-none".OnlyWhen(item.Feature.Hide)))
                    ).AddTo(ul);

                li.Add(new XAttribute("expand", currentFeature.IsAnyOf(feature.WithAllChildren())));

                if (feature.Parent != null)
                {
                    li.Add(new XAttribute("is-side-menu-child", "true"));
                    li.Add(new XAttribute("side-menu-parent", feature.Parent.ID));
                }
                else
                {
                    li.Add(new XAttribute("is-side-menu-child", "false"));
                }

                var link = new XElement("a",
                    new XAttribute("href", item.AddQueryString()),
                    new XAttribute("data-badgeurl", feature.GetBadgeUrl().OrEmpty()),
                    new XAttribute("data-badge-optional", feature.IsBadgeOptional()),
                    new XAttribute("data-service", (feature.Service?.Name).OrEmpty()),
                    new XAttribute("class", "badge-number"),
                    new XElement("i", string.Empty,
                        new XAttribute("class", $"{feature.Icon}"),
                        new XAttribute("aria-hidden", "true")),
                    feature.Title
                    ).AddTo(li);

                if (!item.IsDisabled && !feature.UseIframe)
                    link.Add(new XAttribute("data-redirect", "ajax"));

                var children = FeatureSecurityFilter.GetAuthorizedFeatures(Context.Current.User(), parent: feature);

                if (children.Any()){
                    children=children.OrderBy(child=>child.Feature.Title).ToArray();
                    li.Add(RenderMenu(currentFeature, children));
                }
            }

            return ul;
        }

        string AddQueryString()
        {
            if (IsDisabled) return string.Empty;

            var query = Context.Current.Request().Query;

            var result = Feature.LoadUrl;

            if (Feature.Pass.HasAny() && query.Any())
            {
                var queryStringItems = (from key in Feature.Pass.Split(",")
                                        where query.ContainsKey(key)
                                        select key + "=" + query[key]).ToString("&");

                return $"{result.TrimEnd("/")}{queryStringItems.WithPrefix("?")}";
            }

            return result;
        }

        public async static Task<string> RenderJsonMenu()
        {
            var items = FeatureSecurityFilter.GetAuthorizedFeatures(Context.Current.User());

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };

            return JsonConvert.SerializeObject(await GetAllMenuItems(items), Formatting.None, jsonSerializerSettings);
        }

        public async static Task<HashSet<JsonMenu>> GetAllMenuItems(IEnumerable<AuthroziedFeatureInfo> items)
        {
            var menuITems = new HashSet<JsonMenu>();

            if(items.HasAny()) items = items.OrderBy(x => x.Feature.Title).ToArray();

            foreach (var item in items)
            {
                var feature = item.Feature;

                var children = FeatureSecurityFilter.GetAuthorizedFeatures(Context.Current.User(), parent: feature);

                var sumMenu = new JsonMenu
                {
                    ID = item.Feature.ID,
                    Icon = item.Feature.Icon,
                    Title = item.Feature.Title,
                    LoadUrl = item.AddQueryString(),
                    LogicalPath = feature.ToString(),
                    UseIframe = item.Feature.UseIframe
                };

                if (children.Any())
                {
                    sumMenu.Children = await GetAllMenuItems(children);
                }

                menuITems.Add(sumMenu);
            }

            return menuITems;
        }

        public class JsonMenu
        {
            public Guid ID { get; set; }
            public string Title { get; set; }
            public string Icon { get; set; }
            public string LoadUrl { get; set; }
            public string LogicalPath { get; set; }
            public bool UseIframe { get; set; }
            public HashSet<JsonMenu> Children { get; set; }
        }
    }
}