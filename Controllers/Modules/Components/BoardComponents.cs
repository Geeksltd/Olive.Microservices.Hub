using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Olive.Mvc;
using vm = ViewModel;

namespace ViewComponents
{
#pragma warning disable
    public partial class BoardComponents : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.BoardComponents info)
        {
            return View(await Bind<vm.BoardComponents>(info));
        }
    }
}

namespace Controllers
{
#pragma warning disable
    public partial class BoardComponentsController : BaseController
    {
    }
}

namespace ViewModel
{
#pragma warning disable
    [BindingController(typeof(Controllers.BoardComponentsController))]
    public partial class BoardComponents : IViewModel
    {
        public string FeatureId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public int MinColumnWidth { get; set; } = 350;
    }
}