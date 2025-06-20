using firstProject.ApplicationDbContext;
using firstProject.DTO;
using firstProject.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace firstProject.Controllers
{
    [Route("api/")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LoginController> _logger;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;
        private readonly DB _db;
        public LoginController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<LoginController> logger,EmailService emailService, IConfiguration config,DB db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailService = emailService;
            _config = config;
            _db = db;
        }

        //تسجيل دخول المستخدم
        [HttpPost("Login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginDTO loginDTO)
        {
           
            if (loginDTO == null || string.IsNullOrEmpty(loginDTO.Email) || string.IsNullOrEmpty(loginDTO.Password))
            {
                return BadRequest(new ApiResponse { Message = "يجب إدخال البريد الإلكتروني وكلمة المرور" });
            }

            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == loginDTO.Email);
                if (user == null || await _userManager.IsLockedOutAsync(user) || user.isBlocked == true || user.isActive == false)
                {
                    return Unauthorized(new ApiResponse { Message = "فشل في تسجيل الدخول" });
                }

                var signInResult = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password!, lockoutOnFailure: true);

                if (!signInResult.Succeeded)
                {
                    return Unauthorized(new ApiResponse { Message = "فشل في تسجيل الدخول" });
                }


                var verifyCode = await new Functions(_userManager,_db).GenerateVerifyCode(user, "VerifyLogin")!;
                
                var Body = string.Format(@"
<!DOCTYPE html>
<html lang=""ar"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>تأكيد تسجيل الدخول - Takles Tech</title>
</head>
<body style=""font-family: Arial, sans-serif; color: #333; text-align: center; padding: 20px;"">
    <div style=""max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 10px; padding: 20px; background-color: #f9f9f9;"">
        <h2 style=""color: #28a745;"">تأكيد تسجيل الدخول</h2>
        <p style=""font-size: 16px;"">مرحبًا،</p>
        <p style=""font-size: 16px;"">لقد تم طلب تسجيل الدخول إلى حسابك في <strong>Takles Tech</strong>.</p>
        <p style=""font-size: 16px;"">لإكمال تسجيل الدخول، يرجى إدخال رمز التحقق التالي:</p>
        <p style=""display: inline-block; padding: 12px 25px; background-color: #28a745; color: white; text-decoration: none; font-size: 22px; border-radius: 5px; font-weight: bold; margin-top: 10px;"">
        {0}
        </p>
        <p style=""font-size: 16px; margin-top:20px;"">يرجى إدخال هذا الكود في صفحة التحقق الخاصة بموقعنا.</p>
        <p style=""font-size: 14px; color: #777; margin-top: 20px;"">إذا لم تحاول تسجيل الدخول، يمكنك تجاهل هذه الرسالة أو التواصل معنا لتأمين حسابك.</p>
        <hr style=""margin:30px 0;""/>
        <p style=""font-size: 14px; color: #777;"">
            مع تحيات<br/>
            فريق <strong>Takles Tech</strong><br/>
            <a href=""https://taklestech.com"" target=""_blank"">taklestech.com</a><br/>
            للدعم الفني: <a href=""mailto:support@taklestech.com"">support@taklestech.com</a>
        </p>
    </div>
</body>
</html>", verifyCode);
                var result = await _emailService.SendEmailAsync(user.Email!, "تأكيد تسجيل الدخول", Body);
                var roles = await _userManager.GetRolesAsync(user);
                var rolesString = string.Join(", ", roles);
                user.lastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                var tokenService = new Token_verfy(_config, _userManager, _logger);
                var generatedToken = await tokenService.GenerateToken(user);
                Response.Cookies.Append("token", generatedToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    Domain = ".takhleesak.com",
                });

                return Ok(new ApiResponse
                {
                    Message = "تم تسجيل الدخول بنجاح",
                    Data = rolesString,
                    State = "VerifyLogin"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ أثناء تسجيل الدخول");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "فشل في تسجيل الدخول"+ex.Message});
            }
        }
    }
}
