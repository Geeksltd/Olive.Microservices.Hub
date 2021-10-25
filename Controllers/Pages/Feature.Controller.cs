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
    public partial class FeatureController : BaseController
    {
        [Route("")]
        [Route("{l1}")]
        [Route("{l1}/{l2}")]
        [Route("{l1}/{l2}/{l3}")]
        [Route("{l1}/{l2}/{l3}/{l4}")]
        [Route("{l1}/{l2}/{l3}/{l4}/{l5}")]
        [Route("{l1}/{l2}/{l3}/{l4}/{l5}/{l6}")]
        [Route("{l1}/{l2}/{l3}/{l4}/{l5}/{l6}/{l7}")]
        [Route("{l1}/{l2}/{l3}/{l4}/{l5}/{l6}/{l7}/{l8}")]
        public async Task<ActionResult> Index(vm.FeatureView info)
        {
            //if (!User.Identity.IsAuthenticated)
            //{
            //    return Redirect(Url.Index("Login", new { ReturnUrl = Url.Current() }));
            //}

            return Execute(info);

            return View(info);
        }
    }
}

namespace ViewModel
{

#pragma warning disable
    public partial class FeatureView : IViewModel
    {
        [ValidateNever]
        public Feature Item { get; set; }
    }
}