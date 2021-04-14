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

namespace Controllers
{
    
#pragma warning disable
    public partial class EverythingController : BaseController
    {
        [Route("everything")]
        public async Task<ActionResult> Index(vm.EverythingPage_Everything info)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect(Url.Index("Login", new { ReturnUrl = Url.Current() }));
            }

            ViewData["LeftMenu"] = "FeaturesSideMenu";

            return View(info);
        }

        [NonAction, OnBound]
        public async Task OnBound(vm.EverythingPage_Everything info)
        {
            info.Markup = await AuthroziedFeatureInfo.RenderFullMenu();
            // Load Javascript file
            JavaScript(JavascriptModule.Relative("/scripts/featuresMenu/FullMenuFiltering.js"), "run()");
        }
    }
}

namespace ViewModel
{
    
#pragma warning disable
    public partial class EverythingPage_Everything : IViewModel
    {
        [ReadOnly(true)]
        public string InstantSearch { get; set; }

        [ReadOnly(true)]
        public string Markup { get; set; }
    }
}