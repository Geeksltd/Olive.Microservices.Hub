using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Olive.Mvc;
using vm = ViewModel;

namespace ViewComponents
{


    public partial class TaskFrameModule : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.TaskFrameModule info)
        {
            return View(await Bind<vm.TaskFrameModule>(info));
        }
    }
}

namespace Controllers
{


    public partial class TaskFrameModuleController : BaseController
    {
    }
}

namespace ViewModel
{


    [BindingController(typeof(Controllers.TaskFrameModuleController))]
    public partial class TaskFrameModule : IViewModel
    {
    }
}