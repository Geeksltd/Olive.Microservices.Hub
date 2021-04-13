using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Olive;
using Olive.Microservices.Hub;
using Olive.Mvc;

namespace ViewModel
{
    partial class FeatureView
    {
        public string RequestPath, Path, HostAndPath, ExecutionUrl, NewQueryString;

        internal void ReadPath(HttpRequest request)
        {
            RequestPath = request.ToPathAndQuery();
            Path = RequestPath.TrimStart("/");

            var open = "%5B";
            var close = "%5D";

            if (Path.StartsWith(open) && Path.Contains(close))
            {
                Path = Path.Substring(3);
                Path = Path.Remove(Path.IndexOf(close), 3);
            }

            HostAndPath = request.RootUrl() + request.Path.ToString().TrimStart("/");

            if (Path.IsEmpty())
            {
                RequestPath = Path = "dashboard/home.aspx";
                HostAndPath = request.RootUrl() + Path.TrimStart("/");
            }
        }

        internal Feature FindFeature()
        {
            if (Feature.All == null)
                throw new Exception("Feature.All has not been initialised.");

            return
                Feature.All.FirstOrDefault(x => x.LoadUrl == Path.ToLower().EnsureStartsWith("/"))
                ?? Feature.FindByHubUrl(Path) ?? Feature.FindByAbsoluteImplementationUrl(HostAndPath)
                ?? Feature.FindBySubFeaturePath(Path);
        }
    }
}

namespace Controllers
{
    partial class FeatureController
    {
        [NonAction, OnBound]
        public async Task OnBound(ViewModel.FeatureView info)
        {
            info.ReadPath(Request);
            FeatureContext.ViewingFeature = info.Item = info.FindFeature();
        }

        ActionResult Execute(ViewModel.FeatureView info)
        {
            //if (info.RequestPath.StartsWith("/Hub/", caseSensitive: false))
            //    return Redirect(info.RequestPath.Substring(4));

            ViewData["Title"] = info.Item?.GetFullPath();

            if (info.Item == null)
            {
                var path = info.RequestPath.TrimStart("/");

                var service = Service.All.FirstOrDefault(s => path.StartsWith(s.Name, caseSensitive: false));
                if (service is null) return Redirect("/");

                var actualRelativeUrl = path.Substring(service.Name.Length).TrimStart("/");

                if (actualRelativeUrl.IsEmpty())
                    return Redirect("/under/" + service.Name);
                else
                    JavaScript(new JavascriptService("hub", "go", service.GetHubImplementationUrl(actualRelativeUrl), service.UseIframe));
            }
            else if (!User.CanSee(info.Item))
            {
                return Redirect("/Unauthorized/" + info.Item.ID);
            }
            else
            {
                if (info.RequestPath.Contains("/%5B"))
                    return Redirect($"/{info.RequestPath.Remove("/%5B").Remove("%5D")}");

                var url = info.RequestPath.Split('/')
                .Select((v, i) => i == 1 ? v.EnsureStartsWith("[").EnsureEndsWith("]") : v)
                .ToString("/");
                // var url = "/[" + info.Item.Service.Name people"]" + info.RequestPath

                JavaScript(new JavascriptService("hub", "go", url, info.Item.UseIframe));

                if (info.Item.ImplementationUrl.HasValue() && info.Item.UseIframe)
                {
                    JavaScript(new JavascriptService("featuresMenu", "show", info.Item.ID));
                }
            }

            ViewData["LeftMenu"] = "FeaturesSideMenu";
            return View(info);
        }
    }
}