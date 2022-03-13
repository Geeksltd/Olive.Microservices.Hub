using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Olive.Mvc;
using vm = ViewModel;

namespace ViewComponents
{
#pragma warning disable
    public partial class MainMenu : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.MainMenu info)
        {
            return View(await Bind<vm.MainMenu>(info));
        }
    }
}

namespace Controllers
{
#pragma warning disable
    public partial class MainMenuController : BaseController
    {
        [NonAction, OnBound]
        public async Task OnBound(vm.MainMenu info)
        {
            info.ActiveItem = GetActiveItem(info);
        }

        [NonAction]
        public string GetActiveItem(vm.MainMenu info) => null;
    }
}

namespace ViewModel
{
#pragma warning disable
    [BindingController(typeof(Controllers.MainMenuController))]
    public partial class MainMenu : IViewModel
    {
        public string ActiveItem { get; set; }
    }
}