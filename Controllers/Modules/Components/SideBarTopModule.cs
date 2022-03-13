using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Olive.Mvc;
using vm = ViewModel;

namespace ViewComponents
{
#pragma warning disable
    public partial class SideBarTopModule : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.SideBarTopModule info)
        {
            return View(await Bind<vm.SideBarTopModule>(info));
        }
    }
}

namespace Controllers
{
#pragma warning disable
    public partial class SideBarTopModuleController : BaseController
    {
    }
}

namespace ViewModel
{
#pragma warning disable
    [BindingController(typeof(Controllers.SideBarTopModuleController))]
    public partial class SideBarTopModule : IViewModel
    {
    }
}