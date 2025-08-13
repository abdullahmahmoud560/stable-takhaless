using firstProject.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace firstProject.Controllers
{
    [Route("api/")]
    [ApiController]
    public class CheckerCOntroller : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public CheckerCOntroller(UserManager<User> userManager)
        {
            _userManager = userManager;
        }


        [Authorize]
        [HttpGet("Checker")]
        public async Task<IActionResult> Checker()
        {
           return Ok();
        }
    }
}
