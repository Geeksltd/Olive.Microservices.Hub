﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Olive;
using Olive.Entities;
using Olive.Microservices.Hub;
using Olive.Mvc;
using vm = ViewModel;

namespace Controllers
{
    [Authorize(Roles = "Director")]

#pragma warning disable
    public partial class AdminFeaturesController : BaseController
    {
        [Route("admin/features")]
        public async Task<ActionResult> Index(vm.FeaturesList info)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect(Url.Index("Login", new { ReturnUrl = Url.Current() }));
            }

            ViewData["LeftMenu"] = "FeaturesSideMenu";

            return View(info);
        }

        [NonAction, OnBound]
        public async Task OnBound(vm.FeaturesList info)
        {
            info.Items = await GetSource(info)
                .Select(item => new vm.FeaturesList.ListItem(item)).ToList();
        }

        [NonAction]
        async Task<IEnumerable<Feature>> GetSource(vm.FeaturesList info)
        {
            var result = Feature.All;

            if (info.InstantSearch.HasValue())
            {
                var keywords = info.InstantSearch.Split(' ').Trim().ToArray();
                result = result.Where(item => item.ToString("F").ContainsAll(keywords, caseSensitive: false));
            }

            return result;
        }
    }
}

namespace ViewModel
{
#pragma warning disable
    public partial class FeaturesList : IViewModel
    {
        // Search filters
        public string InstantSearch { get; set; }

        [ReadOnly(true)]
        public List<ListItem> Items = new List<ListItem>();

        public partial class ListItem : IViewModel
        {
            public ListItem(Feature item) => Item = item;

            [ValidateNever]
            public Feature Item { get; set; }
        }
    }
}