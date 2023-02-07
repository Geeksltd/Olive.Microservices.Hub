using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Olive.Mvc;
using vm = ViewModel;

namespace ViewComponents
{
#pragma warning disable
    public partial class SideBarRightModule : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.SideBarRightModule info)
        {
            return View(await Bind<vm.SideBarRightModule>(info));
        }
    }
}

namespace Controllers
{
#pragma warning disable
    public partial class SideBarRightModuleController : BaseController
    {
    }
}

namespace ViewModel
{
#pragma warning disable
    [BindingController(typeof(Controllers.SideBarRightModuleController))]
    public partial class SideBarRightModule : IViewModel
    {
    }
}