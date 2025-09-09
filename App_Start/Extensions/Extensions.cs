using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Olive;
using Olive.Microservices.Hub;
using Olive.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace System
{
    public static class Extensions
    {
        static int Timeout => Config.Get<int>("Authentication:Cookie:Timeout");
        static int MobileTimeout => Config.Get<int>("Authentication:Cookie:TimeoutMobile");

        public static async Task<string> LogOn(this PeopleService.UserInfo @this)
        {
            var mobile = Context.Current.Request().IsSmartPhone();

            var loggingInfo = new GenericLoginInfo
            {
                DisplayName = @this.DisplayName,
                Email = @this.Email,
                ID = @this.ID.ToString(),
                Roles = @this.Roles.Split(',').Trim().ToArray(),
                Timeout = mobile ? MobileTimeout.Minutes() : Timeout.Minutes()
            };

            TryAddJwtToken(loggingInfo, mobile);

            await loggingInfo.LogOn(remember: mobile);

            return Context.Current.Http().GetAuthToken();
        }

        public static string GetAuthToken(this HttpContext httpContext)
        {
            var cookieOptions = httpContext.RequestServices.GetService<IOptionsMonitor<CookieAuthenticationOptions>>();
            var cookieName = cookieOptions?.Get(CookieAuthenticationDefaults.AuthenticationScheme).Cookie.Name;

            if (cookieName.IsEmpty())
            {
                return "";
            }

            var authCookie = httpContext.Response.Headers["Set-Cookie"]
            .FirstOrDefault(h => h.StartsWith($"{cookieName}="));

            var token = authCookie?.Split(';')[0].Split('=')[1];
            return token;
        }

        public static async Task<PeopleService.UserInfo> LoadUser(this ClaimsPrincipal principal)
        {
            var email = Context.Current.User().GetEmail();
            if (email.IsEmpty()) return null;
            return await Context.Current.Database().FirstOrDefault<PeopleService.UserInfo>(x => x.Email == email);
        }

        private static void TryAddJwtToken(GenericLoginInfo loggingInfo, bool mobile)
        {
            try
            {
                var jwt = loggingInfo.CreateJwtToken(remember: mobile);
                if (jwt.IsEmpty()) return;

                var cookieName = Config.Get("Authentication:JWT:Cookie:Name");
                if (cookieName.IsEmpty()) return;

                var cookieDomain = Config.Get("Authentication:JWT:Cookie:Domain").Or(Config.Get("Authentication:Cookie:Domain"));
                if (cookieDomain.IsEmpty()) return;

                Context.Current.Http().Response.Cookies.Append(cookieName, jwt, new CookieOptions
                {
                    Domain = cookieDomain,
                    MaxAge = loggingInfo.Timeout,
                    Secure = Context.Current.Request().IsHttps,
                    HttpOnly = false,
                    SameSite = SameSiteMode.Lax
                });
            }
            catch (Exception e)
            {
                // ignore
            }
        }

        public static Feature[] SubItems(this Feature @this)
        {
            if (@this == null)
                return Feature.All.Where(x => x.Parent == null).ToArray();
            else return @this.GetAllChildren().Cast<Feature>().ToArray();
        }

        public static bool IsSmartPhone(this HttpRequest @this)
        {
            var agent = @this.Headers["User-Agent"].ToString("|").ToLowerOrEmpty();
            return agent.ContainsAny(new[] { "iphone", "android" });
        }

        public static bool IsUAT(this Microsoft.AspNetCore.Hosting.IWebHostEnvironment @this) => @this.EnvironmentName == "UAT";

        public static bool IsProduction(this Microsoft.AspNetCore.Hosting.IWebHostEnvironment @this) => @this.EnvironmentName == "Production";

        static string HeaderButtonTargetAttr(HeaderButton button)
        {
            if (button.Target == "PopUp")
                return "target=\"$modal\"";
            else if (button.Target == "NewWindow")
                return "target=\"_blank\"";
            else if (button.Target == "Ajax")
                return "data-redirect=\"ajax\"";

            return "";
        }

        static string RenderHeaderButton(HeaderButton button, string cssClass = "")
        {
            if (button.Divider) return "<div class=\"dropdown-divider\"></div>";

            var attr = HeaderButtonTargetAttr(button);

            var url = button.Url.Contains("://") ? button.Url : Microservice.Of("Hub").Url(button.Url);
            var style = button.Colour.IsEmpty() ? "" : $"style='color:{button.Colour}'";
            return @$"
                    <a class=""{cssClass}"" href=""{url}"" {attr} {style} title='{button.Title}'>
                        <i class=""{button.Icon}""></i> {button.Text}
                    </a> ";
        }

        public static string RenderHeaderButtons(this IHtmlHelper htmlHelper)
        {
            var buttons = Config.Bind<List<HeaderButton>>("HeaderButtons:Buttons");
            if (buttons == null || buttons.None()) return "";
            var result = "";
            var user = Context.Current.User();

            foreach (var button in buttons.Where(x => x.Roles.IsEmpty() || x.Roles.Split(",").ContainsAny(user.GetRoles().ToArray())))
                result += RenderHeaderButton(button);

            return $"<div class=\"sidebar-top-module-profile-buttons\">{result}</div>";
        }
        public static List<string> GetHeaderButtons(this IHtmlHelper htmlHelper, string cssClass)
        {
            var result = new List<string>();
            var buttons = Config.Bind<List<HeaderButton>>("HeaderButtons:Buttons");
            if (buttons == null || buttons.None()) return result;
            var user = Context.Current.User();

            foreach (var button in buttons.Where(x => x.Roles.IsEmpty() || x.Roles.Split(",").ContainsAny(user.GetRoles().ToArray())))
                result.Add(RenderHeaderButton(button, cssClass));

            return result;
        }
    }
}