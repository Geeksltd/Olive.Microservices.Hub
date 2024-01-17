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
    public partial class FeatureController : BaseController
    {
        [Route("")]
        [Route("{l1}")]
        [Route("{l1}/{l2}")]
        [Route("{l1}/{l2}/{l3}")]
        [Route("{l1}/{l2}/{l3}/{l4}")]
        [Route("{l1}/{l2}/{l3}/{l4}/{l5}")]
        [Route("{l1}/{l2}/{l3}/{l4}/{l5}/{l6}")]
        [Route("{l1}/{l2}/{l3}/{l4}/{l5}/{l6}/{l7}")]
        [Route("{l1}/{l2}/{l3}/{l4}/{l5}/{l6}/{l7}/{l8}")]
        public async Task<ActionResult> Index(vm.FeatureView info)
        {
            if (!User.Identity.IsAuthenticated && ($"{Request.Path}".IsEmpty() || $"{Request.Path}" == "/"))
            {
                return Redirect(Url.Index("Login", new { ReturnUrl = Url.Current() }));
            }
            info.NoNav = Request.Query.ContainsKey("_nav") && Request.Query["_nav"]=="no";
            return await Execute(info);
        }
    }
}

namespace ViewModel
{
#pragma warning disable
    public partial class FeatureView : IViewModel
    {
        [ValidateNever]
        public Feature Item { get; set; }
        public bool NoNav { get; set; }
    }
}