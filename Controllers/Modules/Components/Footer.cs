using System.Collections.Generic;
using System.Threading.Tasks;
using Olive.Microservices.Hub.Domain.Theme.Contracts;
using Microsoft.AspNetCore.Mvc;
using Olive;
using Olive.Mvc;
using vm = ViewModel;

namespace ViewComponents
{
#pragma warning disable
	public partial class Footer : ViewComponent
	{
        private readonly IThemeProvider _themeProvider;

        public Footer(IThemeProvider themeProvider)
        {
            _themeProvider = themeProvider;
        }

		public async Task<IViewComponentResult> InvokeAsync(vm.Footer info)
		{
			var email = Context.Current.User().GetEmail();

			var user = await Context.Current.Database().FirstOrDefault<PeopleService.UserInfo>(x => x.Email == email);
			var userRoles = user.Roles.Split(',');

			var sidebarProfileUrl = await _themeProvider.GetSidebarProfileUrl(userRoles, new Dictionary<string, string>
			{
				{ "ID", user?.ID.ToString().OrEmpty() },
				{ "EMAIL", email },
			});

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