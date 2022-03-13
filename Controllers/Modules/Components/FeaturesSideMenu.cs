using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Olive.Microservices.Hub;
using Olive.Mvc;
using vm = ViewModel;

namespace ViewComponents
{
#pragma warning disable
    public partial class FeaturesSideMenu : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.FeaturesSideMenu info)
        {
            return View(await Bind<vm.FeaturesSideMenu>(info));
        }
    }
}

namespace Controllers
{
#pragma warning disable
    public partial class FeaturesSideMenuController : BaseController
    {
        [NonAction, OnBound]
        public async Task OnBound(vm.FeaturesSideMenu info)
        {
            info.Markup = (AuthroziedFeatureInfo.RenderMenu(FeatureContext.ViewingFeature)).ToString();
        }
    }
}

namespace ViewModel
{
#pragma warning disable
    [BindingController(typeof(Controllers.FeaturesSideMenuController))]
    public partial class FeaturesSideMenu : IViewModel
    {
        [ReadOnly(true)]
        public string Markup { get; set; }
    }
}