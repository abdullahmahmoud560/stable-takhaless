using Application.Interface;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Shared.DataTransferObject;

namespace firstProject.Controllers
{
    [Route("api/")]
    [ApiController]
    public class VerifyEmail : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        private readonly IUserService _userService;
        public VerifyEmail(IServiceManager serviceManager, IUserService userService)
        {
            _serviceManager = serviceManager;
            _userService = userService;
        }

        [Authorize]
        [HttpPost("VerifyCode")]
        public async Task<IActionResult> VerifyCode(VerifyCode verifyCode)
        {
            var Id = User.FindFirstValue("ID");
            if (string.IsNullOrEmpty(Id)||string.IsNullOrEmpty(verifyCode.Code) ||string.IsNullOrEmpty(verifyCode.typeOfGenerate))
                return BadRequest(new ApiResponse { Message = "برجاء ملئ جميع الحقول" });

            var result = await _userService.VerifyCode(verifyCode,Id);
            if (result.Success)
            {
                var Role = await _userService.GetRole(result.Error);
                var rolesString = string.Join(", ", Role);
                var AccessToken = await _serviceManager.TokenService.GenerateAccessToken(result.Error!);
                if (!AccessToken.Success)
                    return BadRequest(new ApiResponse { Message = AccessToken.Error });

                if (verifyCode.typeOfGenerate == "VerifyUserEmail" || verifyCode.typeOfGenerate == "VerifyCompanyEmail" || verifyCode.typeOfGenerate == "VerifyBrokerEmail")
                {
                    var check = await _userService.ActiveEmail(result.Error);
                    if (!check.Success)
                    {
                        return BadRequest(new { Message = "فشل اثناء تفعيل البريد الإلكتروني" });
                    }

                    CookieHelper.RemoveTokenCookie(Response);
                    CookieHelper.SetTokenCookieInDays(Response, AccessToken.Error, 7);

                    return Ok(new ApiResponse{ Message = "تم تأكيد البريد الإلكتروني بنجاح" ,Data = rolesString});
                }

                CookieHelper.RemoveTokenCookie(Response);
                CookieHelper.SetTokenCookieInDays(Response, AccessToken.Error, 7);
                return Ok(new ApiResponse { Message = "تم التأكيد بنجاح", Data = rolesString });

            }
            return BadRequest(new { Message = "فشل اثناء تأكيد الكود" });
        }

        [Authorize]
        [HttpPost("VerifyCode-Mobile")]
        public async Task<IActionResult> VerifyCodeMobile(VerifyCode verifyCode)
        {
            var Id = User.FindFirstValue("ID");
            if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(verifyCode.Code) || string.IsNullOrEmpty(verifyCode.typeOfGenerate))
                return BadRequest(new ApiResponse { Message = "برجاء ملئ جميع الحقول" });

           var result = await _userService.VerifyCode(verifyCode, Id!);
            if (!result.Success)
                return BadRequest(new ApiResponse { Message = "فشل أثناء التأكيد" });

            var tokenService = await _serviceManager.TokenService.GenerateAccessToken(result.Error);
            var Role = await _userService.GetRole(result.Error);
            var rolesString = string.Join(", ", Role);

            if (verifyCode.typeOfGenerate == "VerifyUserEmail" || verifyCode.typeOfGenerate == "VerifyCompanyEmail" || verifyCode.typeOfGenerate == "VerifyBrokerEmail")
            {
                var active = await _userService.ActiveEmail(result.Error);
                if (!active.Success)
                    return BadRequest(new ApiResponse { Message = active.Error });

                var token = await _serviceManager.TokenService.GenerateAccessToken(result.Error);
                return Ok(new ApiResponse { Message = "تم تأكيد البريد الإلكتروني بنجاح", Data = token, State = rolesString });
            }
            var generatedToken = await _serviceManager.TokenService.GenerateAccessToken(result.Error);
            return Ok(new ApiResponse { Message = "تم الـتأكيد بنجاح", Data = generatedToken.Error, State = rolesString });

        }

        [Authorize]
        [HttpGet("Resend-Code/{TypeOfGenerate}")]
        public async Task<IActionResult> resendCode(string TypeOfGenerate)
        {
            var ID = User.FindFirstValue("ID");
            if (string.IsNullOrEmpty(ID))
                return BadRequest(new ApiResponse { Message = "المستخدم غير موجود" });

            var result = await _userService.FindByIdAsync(ID!);
            if (result == null)
            {
                return BadRequest(new ApiResponse { Message = "المستخدم غير موجود" });
            }

            var verifyCode = await _serviceManager.FunctionService.GenerateVerifyCode(result.Email!, TypeOfGenerate);
            if (!verifyCode.All(c => char.IsDigit(c)))
                return BadRequest(new ApiResponse{ Message = verifyCode });

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
            var send = await _serviceManager.EmailService.SendEmailAsync(result!.Email!, "طلب رمز التحقق مرة أخري", Body);
            if(!send.Success)
                return BadRequest(new ApiResponse { Message = send.Error });
            return Ok(new ApiResponse { Message = "تم ارسال الكود بنجاح" });
        }
    }
}
