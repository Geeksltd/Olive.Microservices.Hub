using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Olive.Mvc;
using vm = ViewModel;

namespace ViewComponents
{
#pragma warning disable
    public partial class SideBarIconsModule : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.SideBarIconsModule info)
        {
            return View(await Bind<vm.SideBarIconsModule>(info));
        }
    }
}

namespace Controllers
{
#pragma warning disable
    public partial class SideBarIconsModuleController : BaseController
    {
    }
}

namespace ViewModel
{
#pragma warning disable
    [BindingController(typeof(Controllers.SideBarIconsModuleController))]
    public partial class SideBarIconsModule : IViewModel
    {
    }
}