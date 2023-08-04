using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Olive;
using Olive.Microservices.Hub;
using Olive.Mvc;
using vm = ViewModel;
using Microsoft.Extensions.Configuration;

namespace ViewComponents
{
#pragma warning disable
    public partial class Footer : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.Footer info)
        {
            var email = Context.Current.User().GetEmail();

            var user = await Context.Current.Database().FirstOrDefault<PeopleService.UserInfo>(x => x.Email == email);
            var userRoles = user.Roles.Split(',');

            var sidebarProfileUrl = GetSidebarProfileUrl(user?.ID.ToString().OrEmpty(), email, userRoles);

            info = new vm.Footer()
            {
                Email = email,
                UserImage = user?.ImageUrl,
                PrimaryDISCColour = user?.PrimaryDISCColour.Or("transparent"),
                SecondaryDISCColour = user?.SecondaryDISCColour.Or("transparent"),
                ProfileUrl = sidebarProfileUrl,
            };

            return View(info);
        }

        private string GetSidebarProfileUrl(string userId, string email, string[] userRoles)
        {
            var sidebarProfileUrl = "";

            var roles = Config.GetSection("SidebarProfileUrl:Roles")
                .GetChildren()
                  .ToDictionary(x => x.Key, x => x.Value);

            if (roles != null)
                sidebarProfileUrl = TryGetSidebarProfileUrlByRole(roles, userRoles);

            if (sidebarProfileUrl.IsEmpty())
                sidebarProfileUrl = Config.Get<string>("SidebarProfileUrl:Default", "https://hub.%DOMAIN%/person/%EMAIL%");

            return sidebarProfileUrl.Replace("%EMAIL%", email).Replace("%ID%", userId);
        }

        private string TryGetSidebarProfileUrlByRole(Dictionary<string, string> roles, string[] userRoles)
        {
            foreach (var keyValue in roles)
                if (userRoles.Any(a => a.Equals(keyValue.Key, false)))
                    return keyValue.Value;
            
            return null;
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
        public string ProfileUrl { get; set; }
        public string UserImage { get; set; }
        public string PrimaryDISCColour { get; set; }
        public string SecondaryDISCColour { get; set; }
    }
}