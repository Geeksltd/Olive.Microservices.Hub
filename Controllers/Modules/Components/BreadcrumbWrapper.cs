using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Olive.Mvc;
using vm = ViewModel;

namespace ViewComponents
{
#pragma warning disable
    public partial class BreadcrumbWrapper : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.BreadcrumbWrapper info)
        {
            return View(await Bind<vm.BreadcrumbWrapper>(info));
        }
    }
}

namespace Controllers
{
#pragma warning disable
    public partial class BreadcrumbWrapperController : BaseController
    {
    }
}

namespace ViewModel
{
#pragma warning disable
    [BindingController(typeof(Controllers.BreadcrumbWrapperController))]
    public partial class BreadcrumbWrapper : IViewModel
    {
    }
}