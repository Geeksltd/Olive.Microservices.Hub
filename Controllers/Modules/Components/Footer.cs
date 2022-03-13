using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Olive;
using Olive.Mvc;
using vm = ViewModel;

namespace ViewComponents
{
#pragma warning disable
    public partial class Footer : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.Footer info)
        {
            var email = Context.Current.User().GetEmail();

            var user = await Context.Current.Database().FirstOrDefault<PeopleService.UserInfo>(x => x.Email == email);

            info = new vm.Footer()
            {
                Email = email,
                UserImage = user?.ImageUrl
            };

            return View(info);
        }
    }
}

namespace Controllers
{
#pragma warning disable
    public partial class FooterController : BaseController
    {
    }
}

namespace ViewModel
{
#pragma warning disable
    [BindingController(typeof(Controllers.FooterController))]
    public partial class Footer : IViewModel
    {
        public string Email { get; set; }
        public string UserImage { get; set; }
    }
}