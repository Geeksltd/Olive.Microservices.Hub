using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Olive.Mvc;
using vm = ViewModel;

namespace ViewComponents
{
    public partial class ProjectFrameModule : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.ProjectFrameModule info)
        {
            return View(await Bind<vm.ProjectFrameModule>(info));
        }
    }
}

namespace Controllers
{
    public partial class ProjectFrameModuleController : BaseController
    {
    }
}

namespace ViewModel
{
    [BindingController(typeof(Controllers.ProjectFrameModuleController))]
    public partial class ProjectFrameModule : IViewModel
    {
    }
}