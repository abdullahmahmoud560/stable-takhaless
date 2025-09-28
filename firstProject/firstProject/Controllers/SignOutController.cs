using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace firstProject.Controllers
{
    [Route("api/")]
    [ApiController]
    public class SignOutController : ControllerBase
    {
        [Authorize]
        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            CookieHelper.RemoveTokenCookie(Response);
            return Ok("تم تسجيل الخروج بنجاح");
        }

    }
}
