using Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [ApiExplorerSettings(IgnoreApi = false)]
    [ApiController]
    [Authorize]
    [Route("api/v1")]
    public class BaseApiController : BaseController
    {
    }
}
