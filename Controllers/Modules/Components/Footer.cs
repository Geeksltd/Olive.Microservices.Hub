using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using Olive;
using Olive.Entities;
using Olive.Mvc;
using Olive.Web;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

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
                UserImage = user.ImageUrl
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