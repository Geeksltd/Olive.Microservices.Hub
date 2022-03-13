using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
            info.Markup = await AuthroziedFeatureInfo.RenderFullMenu();
            // Load Javascript file
            // JavaScript(JavascriptModule.Relative("app/featuresMenu/fullMenuFiltering"), "run()");
            // JavaScript(JavascriptModule.Relative("/scripts/featuresMenu/FullMenuFiltering.js"), "run()");
        }
    }
}

namespace ViewModel
{
#pragma warning disable
    public partial class EverythingPage_Everything : IViewModel
    {
        [ReadOnly(true)]
        public string InstantSearch { get; set; }

        [ReadOnly(true)]
        public string Markup { get; set; }
    }
}