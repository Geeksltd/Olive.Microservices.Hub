using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Logging;
using Olive;
using Olive.Microservices.Hub;
using Olive.Mvc;
using vm = ViewModel;

namespace Controllers
{
#pragma warning disable
    public abstract class BaseLoginController : BaseController
    {
        public abstract Task<IActionResult> OnLoggedOut();

        Microsoft.AspNetCore.Hosting.IWebHostEnvironment Environment;

        public BaseLoginController(Microsoft.AspNetCore.Hosting.IWebHostEnvironment environment)
        {
            Environment = environment;
        }

        // [Route("login/{item:Guid?}")]
        // public async Task<ActionResult> Index(vm.ManualLogin manualLogin, ViewModel.LoginForm loginForm)
        // {
        //    if (Request.Param("returnUrl").IsEmpty())
        //    {
        //        return Redirect(Url.Index("Login", new { ReturnUrl = "/" }));
        //    }

        //    // Remove initial validation messages as well as unintended injected data
        //    ModelState.Clear();

        //    ViewBag.ManualLogin = manualLogin;

        //    return View(loginForm);
        // }

        [HttpGet, Route("logout")]
        public Task<IActionResult> Logout(ViewModel.LoginForm _)
        {
            return OnLoggedOut();
            // await HttpContext.SignOutAsync();
            // return Redirect(Microservice.Of("Dashboard").Url("/login/logout.aspx"));
        }

        [HttpPost("LoginForm/LoginByGoogle")]
        public async Task<ActionResult> LoginByGoogle(vm.LoginForm info)
        {
            // await OAuth.Instance.LoginBy("Google" + "OpenIdConnect".OnlyWhen(Environment.IsUAT()));

            var provider = "Google";//+ "OpenIdConnect".OnlyWhen(Environment.IsUAT());

            if (Context.Current.Request().Param("ReturnUrl").IsEmpty())
            {
                // it's mandatory, otherwise Challenge() immediately returns to Login page
                throw new InvalidOperationException("Request has no ReturnUrl.");
            }
            Log.LogError(" I am here and ready to setup.");
            await Context.Current.Http().ChallengeAsync(provider, new AuthenticationProperties
            {
                RedirectUri = $"/ExternalLoginCallback?ReturnUrl={Context.Current.Request().Param("ReturnUrl")}",
                //RedirectUri = $"/ExternalLoginCallback?ReturnUrl={Context.Current.Request().Param("ReturnUrl")}",
                Items = { new KeyValuePair<string, string>("LoginProvider", provider) }
                

            });

            return JsonActions(info);
        }
    }
}

namespace ViewModel
{
#pragma warning disable
    public partial class LoginForm : IViewModel
    {
        public string Email { get; set; }

        [ReadOnly(true)]
        public string ErrorMessage { get; set; }

        [ValidateNever]
        public User Item { get; set; }
    }
}