using firstProject.DTO;
using firstProject.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace firstProject.Controllers
{
    [Route("api/")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly EmailService _emailService;


        public PasswordController(UserManager<User> userManager, EmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        //هل نسيت كلمة المرور
        [HttpPost("Forget-Password")]
        public async Task<IActionResult> forget([FromBody] ForgetPasswordDTO forgetPasswordDTO)
        {
            var emailExists = await _userManager.Users.AnyAsync(u => u.Email == forgetPasswordDTO.Email);
            var user = await _userManager.Users.FirstOrDefaultAsync(l => l.Email == forgetPasswordDTO.Email);

            if (!emailExists || user!.isBlocked == true)
            {
                return BadRequest(new ApiResponse { Message = "فشل في العملية" });
            }

            var verifyCode = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            if (string.IsNullOrEmpty(verifyCode))
            {
                return BadRequest(new ApiResponse { Message = "فشل في العملية" });
            }
            await _userManager.SetAuthenticationTokenAsync(user, "Default", "ChangePassword", DateTime.UtcNow.ToString());
            Token tokenService = new Token(_userManager);
            var generatedToken = await tokenService.GenerateToken(user);

            Response.Cookies.Append("token", generatedToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(14),
                Domain = ".takhleesak.com",
            });


            // محتوى الرسالة
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
                        <p style=""font-size: 14px; color: #777; margin-top: 20px;"">إذا لم تطلب هذا، يرجى تجاهل هذه الرسالة.</p>
                        <hr style=""margin: 20px 0; border: none; border-top: 1px solid #ddd;"">
                        <p style=""font-size: 12px; color: #555;"">Takhleesak &copy; 2025 - جميع الحقوق محفوظة</p>
                    </div>
                </body>
                </html>", verifyCode);


            await _emailService.SendEmailAsync(forgetPasswordDTO.Email!, "إعادة تعيين كلمة المرور", Body);


            return Ok(new ApiResponse
            {
                Message = "تم إرسال الرسالة بنجاح",
            });
        }

        //تأكيد الرمز
        [Authorize]
        [HttpPost("Reset-Password")]
        public async Task<IActionResult> reset([FromBody] ResetPasswordDTO resetPasswordDTO)
        {
            var ID = User.FindFirstValue("ID");
            var user = await _userManager.FindByIdAsync(ID!);
            if (string.IsNullOrEmpty(ID) || user == null || user.isBlocked == true)
            {
                return Ok(new ApiResponse { Message = "فشل في العملية" });
            }

            var isCodeValid = await _userManager.VerifyTwoFactorTokenAsync(user, "phoneNumber", resetPasswordDTO.Code!);

            var time = await _userManager.GetAuthenticationTokenAsync(user!, "defalut", "ChanagePassword");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, resetToken, resetPasswordDTO.newPassword!);

            if(resetPasswordDTO.newPassword == resetPasswordDTO.Confirm && result.Succeeded && DateTime.UtcNow - DateTime.Parse(time!) <= TimeSpan.FromMinutes(1))
            {
                await _userManager.ResetAccessFailedCountAsync(user);
                return Ok(new ApiResponse { Message = "تم إعادة تعيين كلمة المرور بنجاح" });
            }
            else
            {
                await _userManager.AccessFailedAsync(user);
                return Ok(new ApiResponse { Message = "فشل في العملية" });
            }
        }
    }
}
