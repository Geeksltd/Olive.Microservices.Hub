using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olive.Mvc;

namespace Controllers
{
    [Authorize(Roles = "Director")]

#pragma warning disable
    public partial class AdminController : BaseController
    {
        [Route("admin")]
        public async Task<ActionResult> Index()
        {
            return Redirect(Url.Index("AdminFeatures"));

            ViewData["LeftMenu"] = "FeaturesSideMenu";

            return new EmptyResult();
        }
    }
}