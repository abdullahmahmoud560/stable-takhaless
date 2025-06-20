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
        private readonly EmailService _emailService;

        public CheckerCOntroller(UserManager<User> userManager, EmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }


        [Authorize]
        [HttpGet("Checker")]
        public async Task<IActionResult> Checker()
        {
           return Ok();
        }
    }
}
