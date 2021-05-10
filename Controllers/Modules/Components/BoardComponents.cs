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
using Olive.Microservices.Hub.BoardComponent;

namespace ViewComponents
{

#pragma warning disable
    public partial class BoardComponents : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.BoardComponents info)
        {
            return View(await Bind<vm.BoardComponents>(info));
        }
    }
}

namespace Controllers
{

#pragma warning disable
    public partial class BoardComponentsController : BaseController
    {
    }
}

namespace ViewModel
{

#pragma warning disable
    [BindingController(typeof(Controllers.BoardComponentsController))]
    public partial class BoardComponents : IViewModel
    {
        public string FeatureId { get; set; }
        public string Type { get; set; }
    }
}