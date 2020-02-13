using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace webApp_urlshortener.Controllers
{
    [Route("Redirector")]
    public class RedirectorController : ControllerBase
    {
        public IActionResult Get()
        {
            return new JsonResult("TODO");
        }
    }
}
