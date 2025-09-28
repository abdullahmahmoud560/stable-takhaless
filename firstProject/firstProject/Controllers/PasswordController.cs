using Application.Interface;
using Infrastructure.Services;
using Infrastructure.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Shared.DataTransferObject;

namespace firstProject.Controllers
{
    [Route("api/")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        private readonly IUserService _userService;


        public PasswordController(IServiceManager serviceManager, IUserService userService)
        {
            _serviceManager = serviceManager;
            _userService = userService;

        }

        [HttpPost("Forget-Password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordDTO forgetDTO)
        {
            forgetDTO.Email = InputSanitizer.SanitizeEmail(forgetDTO.Email);

            var user = await _userService.FindByEmailAsync(forgetDTO.Email);
            if (user == null)
                return BadRequest(new ApiResponse{ Message = "البريد الإلكتروني غير موجود" });

            var token = await _serviceManager.TokenService.GenerateActiveToken(forgetDTO.Email);
            if(!token.Success)
                return BadRequest(new ApiResponse { Message = token.Error });

            var VerifyCode = await _serviceManager.FunctionService.GenerateVerifyCode(user.Email!, "ForgetPassword");
            if (!VerifyCode.All(c => char.IsDigit(c)))
                return BadRequest(new ApiResponse{ Message = VerifyCode });

            var Body = string.Format(@"
    <!DOCTYPE html>
    <html lang=""ar"">
    <head>
        <meta charset=""UTF-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        <title>إعادة تعيين كلمة المرور</title>
    </head>
    <body style=""font-family: Arial, sans-serif; color: #333; text-align: center; padding: 20px; background-color: #f0f8ff;"">
        <div style=""max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 10px; padding: 20px; background-color: #ffffff; box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.1);"">
            <h2 style=""color: #000000;"">إعادة تعيين كلمة المرور</h2>
            <p style=""font-size: 16px;"">لقد طلبت إعادة تعيين كلمة المرور لحسابك:</p>
            <p style=""display: inline-block; padding: 12px 25px; background-color: #000000; color: white; text-decoration: none; font-size: 16px; border-radius: 5px; margin-top: 10px;"">
                رمز التحقق: {0}
            </p>
            <p style=""font-size: 14px; color: #555; margin-top: 5px;"">
                صالح لمدة 5 دقائق فقط
            </p>
            <p style=""font-size: 14px; color: #777; margin-top: 20px;"">إذا لم تطلب هذا، يرجى تجاهل هذه الرسالة.</p>
            <hr style=""margin: 20px 0; border: none; border-top: 1px solid #ddd;"">
            <p style=""font-size: 12px; color: #555;"">Takhleesak &copy; 2025 - جميع الحقوق محفوظة</p>
        </div>
    </body>
    </html>", VerifyCode);

            var generatedToken = await _serviceManager.TokenService.GenerateActiveToken(forgetDTO.Email);
            if (!generatedToken.Success)
                return BadRequest(new ApiResponse { Message = "خطأ اثناء توليد الكود" });

            CookieHelper.SetTokenCookie(Response, generatedToken.Error, 30);

            var send = await _serviceManager.EmailService.SendEmailAsync(forgetDTO.Email!, "إعادة تعيين كلمة المرور", Body);
            if (!send.Success)
                return BadRequest(new ApiResponse { Message = send.Error });

            return Ok(new ApiResponse {Message = "تم إرسال الكود بنجاح",State = "ForgetPassword" });
        }

        [Authorize]
        [HttpPost("Reset-Password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
        {
            var Email = User.FindFirstValue("Email");
            if (string.IsNullOrEmpty(Email))
                return BadRequest(new ApiResponse { Message = "التوكن غير صالح" });
            if (string.IsNullOrEmpty(resetPasswordDTO.Confirm)
                ||string.IsNullOrEmpty(resetPasswordDTO.newPassword))
            {
                return Ok(new ApiResponse { Message = "برجاء ملئ جميع الحقول" });
            }

            if (resetPasswordDTO.newPassword != resetPasswordDTO.Confirm)
                return BadRequest(new ApiResponse { Message = "كلمة المرور غير متطابقة" });

            var result = await _userService.ResetPassword(resetPasswordDTO,Email);
            if(!result.Success) 
                return Ok(new ApiResponse {Message = result.Error});

            return Ok(new ApiResponse { Message ="تم تعيين كلمة المرور بنجاح"});
        }
    }
}
