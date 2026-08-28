using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Olive;
using Olive.Microservices.Hub;
using Olive.Mvc;
using vm = ViewModel;

namespace Controllers
{
#pragma warning disable
    public partial class EverythingController : BaseController
    {
        [Route("everything")]
        public async Task<ActionResult> Index(vm.EverythingPage_Everything info)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect(Url.Index("Login", new { ReturnUrl = Url.Current() }));
            }

            ViewData["LeftMenu"] = "FeaturesSideMenu";

            return View(info);
        }

        [NonAction, OnBound]
        public async Task OnBound(vm.EverythingPage_Everything info)
        {
            info.Groups = FeatureSecurityFilter.GetAuthorizedFeatures(User)
                .Select(x => ToNode(x))
                .OrderByDescending(x => x.TotalCount)
                .ThenBy(x => x.Item.Title)
                .ToList();
        }

        [NonAction]
        vm.EverythingPage_Everything.Node ToNode(AuthroziedFeatureInfo item, string parentPath = null, int depth = 0)
        {
            var path = parentPath.WithSuffix(" > ") + item.Feature.Title;

            return new vm.EverythingPage_Everything.Node
            {
                Item = item.Feature,
                IsDisabled = item.IsDisabled,
                Url = item.AddQueryString(),
                Path = path,
                // The rendered href keeps the [service] token, which the hub resolves.
                // Searching wants it gone, so that "people/absence" matches the
                // feature whose load url is "/[people]/absence".
                SearchUrl = item.Feature.LoadUrl.OrEmpty().Remove("[", "]"),
                Depth = depth,
                Children = FeatureSecurityFilter.GetAuthorizedFeatures(User, parent: item.Feature)
                    .OrderBy(x => x.Feature.Order).ThenBy(x => x.Feature.Title)
                    .Select(x => ToNode(x, path, depth + 1))
                    .ToList()
            };
        }
    }
}

namespace ViewModel
{
#pragma warning disable
    public partial class EverythingPage_Everything : IViewModel
    {
        /// <summary>Named FeatureSearch (not InstantSearch) so that the legacy global
        /// keyup handler in olive.microservices.hubjs (FullMenuFiltering, bound to
        /// '#InstantSearch') cannot fight the filter script in Everything.cshtml.</summary>
        [ReadOnly(true)]
        public string FeatureSearch { get; set; }

        [ReadOnly(true)]
        public List<Node> Groups = new List<Node>();

        public class Node
        {
            [ValidateNever]
            public Feature Item { get; set; }

            /// <summary>The user cannot open this feature itself, but can see some of its children.</summary>
            public bool IsDisabled { get; set; }

            public string Url { get; set; }

            /// <summary>Full logical path, e.g. "Projects &gt; Archive &gt; 2024". Searched against.</summary>
            public string Path { get; set; }

            /// <summary>The load url with the [service] brackets stripped, searched
            /// against alongside <see cref="Path"/>.</summary>
            public string SearchUrl { get; set; }

            /// <summary>0 for a top level group, 1 for its children, and so on.</summary>
            public int Depth { get; set; }

            /// <summary>Rough card height in list row units, for balancing the columns:
            /// one unit per row, plus about two for the header and the gap beneath.
            /// Estimated rather than measured so the script can deal the cards before
            /// the first paint, without reading back layout.</summary>
            public int Weight => TotalCount + 2;

            public List<Node> Children = new List<Node>();

            public int TotalCount => Children.Count + Children.Sum(x => x.TotalCount);

            /// <summary>All descendants, depth first, so they can be rendered as one flat list.</summary>
            public IEnumerable<Node> Descendants()
                => Children.SelectMany(x => new[] { x }.Concat(x.Descendants()));
        }
    }
}
