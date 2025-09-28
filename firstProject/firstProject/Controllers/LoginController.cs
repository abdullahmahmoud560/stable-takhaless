using Application.Interface;
using Infrastructure.Services;
using Infrastructure.Validation;
using Microsoft.AspNetCore.Mvc;
using static Shared.DataTransferObject;

namespace firstProject.Controllers
{
    [Route("api/")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        private readonly IUserService _userService;
        public LoginController(IServiceManager serviceManager,IUserService userService)
        {
           _serviceManager = serviceManager;
            _userService = userService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (loginDTO == null || string.IsNullOrEmpty(loginDTO.Email) || string.IsNullOrEmpty(loginDTO.Password))
            {
                return BadRequest(new ApiResponse { Message = "يجب إدخال البريد الإلكتروني وكلمة المرور" });
            }

            loginDTO.Email = InputSanitizer.SanitizeEmail(loginDTO.Email);

            var result = await _userService.LoginUser(loginDTO);
            if(!result.Success)
                return Ok(new ApiResponse { Message = result.Error });

            var verifyCode = await _serviceManager.FunctionService.GenerateVerifyCode(loginDTO.Email, "VerifyLogin")!;
            if (!verifyCode.All(c => char.IsDigit(c)))
                return BadRequest(new ApiResponse{ Message = verifyCode });

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
    <p style=""font-size: 16px;"">لإكمال تسجيل الدخول، يرجى إدخال رمز التحقق التالي (صالح لمدة 5 دقائق فقط):</p>
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

            var send = await _serviceManager.EmailService.SendEmailAsync(loginDTO.Email!, "تأكيد تسجيل الدخول", Body);
            if (!send.Success)
                return BadRequest(new ApiResponse { Message = send.Error });

            var generatedToken = await _serviceManager.TokenService.GenerateAccessToken(loginDTO.Email);
            if (!generatedToken.Success)
                return BadRequest(new ApiResponse { Message = "خطأ اثناء توليد الكود" });

            CookieHelper.SetTokenCookie(Response, generatedToken.Error, 30);

            return Ok(new ApiResponse{Message = "تم تسجيل الدخول بنجاح",Data = result.Error,State = "VerifyLogin"});}

        [HttpPost("Login-Mobile")]
        public async Task<IActionResult> LoginMobile([FromBody] LoginDTO loginDTO)
        {

            if (string.IsNullOrEmpty(loginDTO.Email) || string.IsNullOrEmpty(loginDTO.Password))
                return BadRequest(new ApiResponse { Message = "يجب إدخال البريد الإلكتروني وكلمة المرور" });

            var result = await _userService.LoginUser(loginDTO);
            if (!result.Success)
                return Ok(new ApiResponse { Message = result.Error });

            var verifyCode = await _serviceManager.FunctionService.GenerateVerifyCode(loginDTO.Email, "VerifyLogin")!;
            if (!verifyCode.All(c => char.IsDigit(c)))
                return BadRequest(new ApiResponse{ Message = verifyCode });

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

            var send = await _serviceManager.EmailService.SendEmailAsync(loginDTO.Email!, "تأكيد تسجيل الدخول", Body);
            if (!send.Success)
                return BadRequest(new ApiResponse { Message = send.Error });

            var generatedToken = await _serviceManager.TokenService.GenerateActiveToken(loginDTO.Email);
            if (!generatedToken.Success)
                return BadRequest(new ApiResponse { Message = generatedToken.Error });

            return Ok(new ApiResponse{Message = generatedToken.Error,Data = result.Error,State = "VerifyLogin"});
        }
    }
}
