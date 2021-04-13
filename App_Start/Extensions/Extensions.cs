using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
    }
}