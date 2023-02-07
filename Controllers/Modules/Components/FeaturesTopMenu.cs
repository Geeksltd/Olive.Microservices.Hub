using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Olive;
using Olive.Microservices.Hub;
using Olive.Mvc;
using vm = ViewModel;

namespace ViewComponents
{
#pragma warning disable
    public partial class FeaturesTopMenu : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.FeaturesTopMenu info)
        {
            return View(await Bind<vm.FeaturesTopMenu>(info));
        }
    }
}

namespace Controllers
{
#pragma warning disable
    public partial class FeaturesTopMenuController : BaseController
    {
        [NonAction, OnPreBound]
        public async Task OnPreBound(vm.FeaturesTopMenu info)
        {
            // Set ViewingFeature
            info.ViewingFeature = FeatureContext.ViewingFeature;

            if (info.Parent.Children.HasAny())
            {
                info.Parent.Children = info.Parent.Children.OrderBy(x => x.Title).ToList();
            }

            // Set the items
            info.Items = info.Parent.Children;

            if (info.Parent.ImplementationUrl.HasValue())
            {
                // Include the parent if it has implementation
                info.Items = new[] { info.Parent }.Union(info.Items);
            }

            if (info.Items.HasAny())
            {
                info.Items = info.Items.OrderBy(x => x.Title).ToList();
            }
        }

        [NonAction, OnBound]
        public async Task OnBound(vm.FeaturesTopMenu info)
        {
            info.ActiveItem = GetActiveItem(info);
        }

        [NonAction]
        public string GetActiveItem(vm.FeaturesTopMenu info)
        {
            return info.Items.Reverse().FirstOrDefault(f => f.WithAllChildren().Contains(info.ViewingFeature))?.ID.ToString();
        }

        void OrderChildren()
        {
        }
    }
}

namespace ViewModel
{
#pragma warning disable
    [BindingController(typeof(Controllers.FeaturesTopMenuController))]
    public partial class FeaturesTopMenu : IViewModel
    {
        [ReadOnly(true)]
        [ValidateNever]
        public Feature Parent { get; set; }

        [ReadOnly(true)]
        [ValidateNever]
        public Feature ViewingFeature { get; set; }

        [ReadOnly(true)]
        public IEnumerable<Feature> Items { get; set; }

        public string ActiveItem { get; set; }
    }
}