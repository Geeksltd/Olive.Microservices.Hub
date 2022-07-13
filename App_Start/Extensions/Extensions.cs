using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Olive;
using Olive.Microservices.Hub;

namespace System
{
    public static class Extensions
    {
        static int Timeout => Config.Get<int>("Authentication:Cookie:Timeout");
        static int MobileTimeout => Config.Get<int>("Authentication:Cookie:TimeoutMobile");

        public static Task LogOn(this PeopleService.UserInfo @this)
        {
            var mobile = Context.Current.Request().IsSmartPhone();

            return new Olive.Security.GenericLoginInfo
            {
                DisplayName = @this.DisplayName,
                Email = @this.Email,
                ID = @this.ID.ToString(),
                Roles = @this.Roles.Split(',').Trim().ToArray(),
                Timeout = mobile ? MobileTimeout.Minutes() : Timeout.Minutes()
            }.LogOn(remember: mobile);
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

        static string RenderHeaderButton(HeaderButton button)
        {
            var attr = HeaderButtonTargetAttr(button);

            var url = button.Url.ToLower().StartsWith("http") ? button.Url : Microservice.Of("Hub").Url(button.Url);
            var style = button.Colour.IsEmpty() ? "" : $"style='color:{button.Colour}' title='{button.Title}'";
            return @$"
        <a class="""" href=""{url}"" {attr} {style}>
            <i class=""{button.Icon}""></i>
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
    }
}