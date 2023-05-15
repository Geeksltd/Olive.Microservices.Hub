using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Olive.Microservices.Hub;
using Olive.Mvc;
using vm = ViewModel;

namespace ViewComponents
{
#pragma warning disable
    public partial class BoardView : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.BoardView info)
        {
            return View(await Bind<vm.BoardView>(info));
        }
    }
}

namespace Controllers
{
#pragma warning disable
    public partial class BoardViewController : BaseController
    {
    }
}

namespace ViewModel
{
#pragma warning disable
    [BindingController(typeof(Controllers.BoardViewController))]
    public partial class BoardView : IViewModel
    {
        public string FeatureId { get; set; }

    }
}