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

        //تسجيل الخروج
        public IActionResult Logout()
        {
            Response.Cookies.Append("token", "", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(-1),
                Domain = ".takhleesak.com",
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.None
            });
            return Ok("تم تسجيل الخروج بنجاح");
        }

    }
}
