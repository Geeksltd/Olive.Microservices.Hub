using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Olive.Microservices.Hub;
using Olive.Mvc;
using vm = ViewModel;

namespace ViewComponents
{
#pragma warning disable
    public partial class FeaturesTopMenuWrapper : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.FeaturesTopMenuWrapper info)
        {
            return View(await Bind<vm.FeaturesTopMenuWrapper>(info));
        }
    }
}

namespace Controllers
{
#pragma warning disable
    public partial class FeaturesTopMenuWrapperController : BaseController
    {
        [NonAction, OnBound]
        public async Task OnBound(vm.FeaturesTopMenuWrapper info)
        {
            info.Markup = "";//(await AuthroziedFeatureInfo.RenderMenuJson()).ToString();

            info.IsVisible = User.Identity.IsAuthenticated;
        }
    }
}

namespace ViewModel
{
#pragma warning disable
    [BindingController(typeof(Controllers.FeaturesTopMenuWrapperController))]
    public partial class FeaturesTopMenuWrapper : IViewModel
    {
        [ReadOnly(true)]
        public bool IsVisible { get; set; }

        [ReadOnly(true)]
        public string Markup { get; set; }
    }
}