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
using Olive.Microservices.Hub;
using ViewModel;
using Microsoft.AspNetCore.Authentication;

namespace Controllers
{

#pragma warning disable
    public partial class UnauthorizedController : BaseController
    {
        [Route("Unauthorized/{feature}")]
        public async Task<ActionResult> Index(Guid? feature)
        {
            ViewData["LeftMenu"] = "FeaturesSideMenu";
            var item = Feature.All.FirstOrDefault(x => x.ID == feature);

            // When user refresh unauthorized page
            if (item != null && User.Identity?.IsAuthenticated == true)
            {
                if (User.CanSee(item)) return Redirect(item.LoadUrl);

                if (await Context.Current.User().LoadUser() is null)
                {
                    return await LoginError("Account not found. Your account may not have been created yet, " +
                        "or there may be an issue with your account. Please contact support if this issue persists.");
                }
            }

            Response.StatusCode = User.Identity?.IsAuthenticated == true ? 403 : 401;
            return View(item);
        }

        /// <summary>Signs the user out and sends them back to the login page with the given message,
        /// exactly as the login page itself does when an invalid email is entered.</summary>
        [NonAction]
        async Task<ActionResult> LoginError(string message)
        {
            await HttpContext.SignOutAsync();

            TempData["LoginErrorMessage"] = message;

            var returnUrl = Request.Param("returnUrl").Or("/");
            return Redirect($"/login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
        }
    }
}

namespace ViewModel
{
    [EscapeGCop("Auto generated code.")]
#pragma warning disable
    public partial class UnauthorizedAccess : IViewModel
    {
        [ValidateNever]
        public Feature Item { get; set; }
    }
}