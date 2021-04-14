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
    public partial class UnauthorizedController : BaseController
    {
        [Route("Unauthorized/{feature}")]
        public async Task<ActionResult> Index(vm.UnauthorizedAccess info)
        {
            ViewData["LeftMenu"] = "FeaturesSideMenu";

            return View(info);
        }
    }
}

namespace ViewModel
{
    
#pragma warning disable
    public partial class UnauthorizedAccess : IViewModel
    {
        [FromRequest("feature")]
        [ValidateNever]
        public Feature Item { get; set; }
    }
}