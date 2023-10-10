using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Olive.Microservices.Hub;
using Olive.Microservices.Hub.Domain.Theme.Contracts;
using Olive.Mvc;
using vm = ViewModel;

namespace ViewComponents
{
#pragma warning disable
    public partial class FeaturesSideMenu : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.FeaturesSideMenu info)
        {
            return View(await Bind<vm.FeaturesSideMenu>(info));
        }
    }
}

namespace Controllers
{
#pragma warning disable
    public partial class FeaturesSideMenuController : BaseController
    {
        private readonly IThemeProvider _themeProvider;

        public FeaturesSideMenuController(IThemeProvider themeProvider)
        {
            _themeProvider = themeProvider;
        }

        [NonAction, OnBound]
        public async Task OnBound(vm.FeaturesSideMenu info)
        {
            var theme = await _themeProvider.GetCurrentTheme();
            info.Markup = (AuthroziedFeatureInfo.RenderMenu(FeatureContext.ViewingFeature, !theme.HideEveryThingMenuItem)).ToString();
        }
    }
}

namespace ViewModel
{
#pragma warning disable
    [BindingController(typeof(Controllers.FeaturesSideMenuController))]
    public partial class FeaturesSideMenu : IViewModel
    {
        [ReadOnly(true)]
        public string Markup { get; set; }
    }
}