using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Olive.Mvc;
using vm = ViewModel;

namespace ViewComponents
{
#pragma warning disable
    public partial class GlobalSearch : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.GlobalSearch info)
        {
            return View(await Bind<vm.GlobalSearch>(info));
        }
    }
}

namespace Controllers
{
#pragma warning disable
    public partial class GlobalSearchController : BaseController
    {
    }
}

namespace ViewModel
{
#pragma warning disable
    [BindingController(typeof(Controllers.GlobalSearchController))]
    public partial class GlobalSearch : IViewModel
    {
    }
}