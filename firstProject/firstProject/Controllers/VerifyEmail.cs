using System.Security.Claims;
using firstProject.ApplicationDbContext;
using firstProject.DTO;
using firstProject.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace firstProject.Controllers
{
    [Route("api/")]
    [ApiController]
    public class VerifyEmail : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly EmailService _emailService;
        private readonly DB _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        public VerifyEmail(UserManager<User> userManager, EmailService emailService , DB dbContext , IHttpContextAccessor httpContext, HttpClient httpClient)
        {
            _userManager = userManager;
            _emailService = emailService;
            _context = dbContext;
            _httpContextAccessor = httpContext;
            _httpClient = httpClient;
        }

        //تاكيد كود التحقق
        [Authorize]
        [HttpPost("VerifyCode")]
        public async Task<IActionResult> VerifyCode(VerifyCode verifyCode)
        {
            var ID = User.FindFirstValue("ID");
            var user = await _userManager.FindByIdAsync(ID!);
            var time = await _context.twoFactorVerify.Where(l => l.UserId == user!.Id && l.Name == verifyCode.typeOfGenerate).Select(g => new { g.Date, g.Value }).FirstOrDefaultAsync();
            bool isCodeValid = BCrypt.Net.BCrypt.Verify(verifyCode.Code, time!.Value);
            bool isCodeExpired = (DateTime.UtcNow - time.Date!.Date) > TimeSpan.FromMinutes(2);

            if (isCodeValid && isCodeExpired && user!.isBlocked == false)
            {
                var tokenService = new Token(_userManager);
                var Role = await _userManager.GetRolesAsync(user);
                var rolesString = string.Join(", ", Role);
                if (verifyCode.typeOfGenerate == "VerifyUserEmail" || verifyCode.typeOfGenerate == "VerifyCompanyEmail" || verifyCode.typeOfGenerate == "VerifyBrokerEmail")
                {
                    user.isActive = true;
                    await _userManager.UpdateAsync(user);
                    var token = await tokenService.GenerateToken(user);
                    Response.Cookies.Append("token", "", new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(-1),
                        //Domain = ".runasp.net",
                        Domain = ".takhleesak.com",
                        Secure = true,
                        HttpOnly = true,
                        SameSite = SameSiteMode.None
                    });
                    Response.Cookies.Append("token", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = DateTime.UtcNow.AddDays(7),
                        Domain = ".takhleesak.com",
                        //Domain = ".runasp.net",
                    });

                    return Ok(new ApiResponse { Message = "تم تأكيد البريد الإلكتروني بنجاح" ,Data =rolesString });
                }
                var generatedToken = await tokenService.GenerateToken(user);
                Response.Cookies.Append("token", "", new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(-1),
                    //Domain = ".runasp.net",
                    Domain = ".takhleesak.com",
                    Secure = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.None
                });

                Response.Cookies.Append("token", generatedToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(7),
                    Domain = ".takhleesak.com",
                    //Domain = ".runasp.net",
                });
                return Ok(new ApiResponse { Message = "تم تأكيد الكود بنجاح", Data = rolesString });
            }

            return BadRequest(new ApiResponse { Message = "فشل" , Data =isCodeValid});
        }

        [Authorize]
        [HttpGet("Resend-Code")]
        public async Task<IActionResult> resendCode()
        {
            var ID = User.FindFirstValue("ID");
            var user = await _userManager.FindByIdAsync(ID!);
            var name = await _context.twoFactorVerify.Where(l => l.UserId == ID).Select(g=>g.Name).FirstOrDefaultAsync();
            var verifyCode = await new Functions(_userManager, _context, _httpContextAccessor, _httpClient).GenerateVerifyCode(user!,name!)!;
            var Body = string.Format(@"
<!DOCTYPE html>
<html lang=""ar"" dir=""rtl"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>رمز التحقق - Takles Tech</title>
</head>
<body style=""font-family: Arial, sans-serif; color: #333; text-align: center; padding: 20px;"">
    <div style=""max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 10px; padding: 20px; background-color: #f9f9f9;"">
        <h2 style=""color: #28a745;"">رمز التحقق الخاص بك</h2>
        <p style=""font-size: 16px;"">مرحبًا،</p>
        <p style=""font-size: 16px;"">لقد تم طلب رمز تحقق من خلال موقع <strong>Takles Tech</strong>.</p>
        <p style=""font-size: 16px;"">يرجى استخدام رمز التحقق التالي:</p>
        <p style=""display: inline-block; padding: 12px 25px; background-color: #28a745; color: white; text-decoration: none; font-size: 22px; border-radius: 5px; font-weight: bold; margin-top: 10px;"">
        {0}
        </p>
        <p style=""font-size: 16px; margin-top:20px;"">يرجى إدخال هذا الرمز في الصفحة المخصصة لذلك على موقعنا لإتمام العملية.</p>
        <p style=""font-size: 14px; color: #777; margin-top: 20px;"">إذا لم تقم بطلب رمز التحقق، يمكنك تجاهل هذه الرسالة أو التواصل معنا لتأمين حسابك.</p>
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
            await _emailService.SendEmailAsync(user!.Email!, "طلب رمز التحقق مرة أخري", Body);
            return Ok(new ApiResponse { Message = "تم ارسال الكود بنجاح"});
        }
    }
}
