using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace firstProject.Controllers
{
    [Route("api/")]
    [ApiController]
    public class CheckerController : ControllerBase
    {
        [Authorize]
        [HttpGet("Checker")]
        public IActionResult Checker()
        {
           return Ok();
        }
    }
}
