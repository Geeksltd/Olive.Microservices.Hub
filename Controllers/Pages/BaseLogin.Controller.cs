using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Olive;
using Olive.Microservices.Hub;
using Olive.Microservices.Hub.Domain.Theme.Contracts;
using Olive.Microservices.Hub.Domain.Theme.LoginLoggers;
using Olive.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using vm = ViewModel;

namespace Controllers
{
#pragma warning disable
    public abstract class BaseLoginController : BaseController
    {
        Microsoft.AspNetCore.Hosting.IWebHostEnvironment Environment;
        protected readonly IThemeProvider ThemeProvider;
        
        public abstract Task<IActionResult> OnLoggedOut();

        public BaseLoginController(Microsoft.AspNetCore.Hosting.IWebHostEnvironment environment, IThemeProvider themeProvider)
        {
            Environment = environment;
            ThemeProvider = themeProvider;
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

            var provider = "Google" + "OpenIdConnect".OnlyWhen(Environment.IsUAT());

            if (Context.Current.Request().Param("ReturnUrl").IsEmpty())
            {
                // it's mandatory, otherwise Challenge() immediately returns to Login page
                throw new InvalidOperationException("Request has no ReturnUrl.");
            }

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
    public partial class AutoLoginForm : IViewModel
    {
        public string Token { get; set; }
    }
}