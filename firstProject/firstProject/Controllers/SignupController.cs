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

    public class SignupController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly EmailService _emailService;
        private readonly DB _db;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Token_verfy _tokenVerifyService;

        public SignupController(UserManager<User> userManager, EmailService emailService, DB db, IHttpContextAccessor httpContext, HttpClient httpClient, Token_verfy tokenVerifyService)
        {
            _userManager = userManager;
            _emailService = emailService;
            _db = db;
            _httpContextAccessor = httpContext;
            _httpClient = httpClient;
            _tokenVerifyService = tokenVerifyService;
        }

        //انشاء حساب للأفراد
        [HttpPost("Register-user")]
        public async Task<IActionResult> Register_user([FromBody] UserDTO registerDTO)
        {
            var existingUser = await _userManager.Users
                .AnyAsync(u => u.Email == registerDTO.Email || u.PhoneNumber == registerDTO.phoneNumber);

            if (existingUser)
            {
                return BadRequest(new ApiResponse { Message = "الرقم او البريد مستخدم بالفعل" });
            }

            var user = new User
            {
                fullName = registerDTO.fullName,
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.phoneNumber,
                UserName = Guid.NewGuid().ToString(),
                Identity = registerDTO.Identity
            };

            if (registerDTO.Password != registerDTO.Confirm)
            {
                return BadRequest(new ApiResponse { Message = "كلمة المرور وتأكيد كلمة المرور غير متطابقتين" });
            }

            var result = await _userManager.CreateAsync(user, registerDTO.Password!);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                var verifyCode = await new Functions(_userManager, _db, _httpContextAccessor, _httpClient).GenerateVerifyCode(user, "VerifyUserEmail")!;
                var Body = string.Format(@"
<!DOCTYPE html>
<html lang=""ar"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>تفعيل الحساب</title>
</head>
<body style=""font-family: Arial, sans-serif; color: #333; text-align: center; padding: 20px;"">
    <div style=""max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 10px; padding: 20px; background-color: #f9f9f9;"">
        <h2 style=""color: #0066cc;"">تفعيل الحساب</h2>
        <p style=""font-size: 16px;"">لقد تلقينا طلبًا لتفعيل حسابك في <strong>Takles Tech</strong>.</p>
        <p style=""font-size: 16px;"">يرجى إدخال الكود التالي لتفعيل حسابك:</p>
        <p style=""display: inline-block; padding: 12px 25px; background-color: #007BFF; color: white; text-decoration: none; font-size: 16px; border-radius: 5px; margin-top: 10px;"">
        رمز التحقق: {0}
        </p>
        <p style=""font-size: 16px;"">يرجى إدخال هذا الكود في الصفحة الخاصة بتفعيل الحساب على موقعنا.</p>
        <p style=""font-size: 14px; color: #777; margin-top: 20px;"">إذا لم تطلب تفعيل الحساب، يرجى تجاهل هذه الرسالة.</p>
    </div>
</body>
</html>", verifyCode);
                var send = await _emailService.SendEmailAsync(user.Email!, "تأكيد بريدك الإلكتروني", Body);
                var generatedToken = await _tokenVerifyService.GenerateToken(user);

                Response.Cookies.Append("token", generatedToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    Domain = ".takhleesak.com",
                    Path = "/"
                });

                return Ok(new ApiResponse { Message = "تم تسجيل حساب الافراد بنجاح", Data = "VerifyUserEmail" });
            }

            return BadRequest(new ApiResponse { Message = "فشل في التسجيل" });
        }

        //انشاء حساب للأعمال
        [HttpPost("Register-company")]
        public async Task<IActionResult> Register_company([FromBody] CompanyDTO companyDTO)
        {
            var existingUser = await _userManager.Users
                .AnyAsync(u => u.Email == companyDTO.Email! || u.PhoneNumber == companyDTO.phoneNumber);

            if (existingUser)
            {
                return BadRequest(new ApiResponse { Message = "الرقم او البريد مستخدم بالفعل" });
            }

            if (companyDTO.Password != companyDTO.Confirm)
            {
                return BadRequest(new ApiResponse { Message = "كلمة المرور وتأكيد كلمة المرور غير متطابقتين" });
            }

            if (string.IsNullOrEmpty(companyDTO.taxRecord) || string.IsNullOrEmpty(companyDTO.InsuranceNumber))
            {
                return BadRequest(new ApiResponse { Message = "يجب تعبئة جميع الحقول المطلوبة" });
            }

            var user = new User
            {
                fullName = companyDTO.fullName,
                Email = companyDTO.Email,
                PhoneNumber = companyDTO.phoneNumber,
                UserName = Guid.NewGuid().ToString(),
                Identity = companyDTO.Identity,
                taxRecord = companyDTO.taxRecord,
                InsuranceNumber = companyDTO.InsuranceNumber,
            };

            var result = await _userManager.CreateAsync(user, companyDTO.Password!);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Company");
                var verifyCode = await new Functions(_userManager, _db, _httpContextAccessor, _httpClient).GenerateVerifyCode(user, "VerifyCompanyEmail")!;
                var Body = string.Format(@"
<!DOCTYPE html>
<html lang=""ar"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>تفعيل الحساب</title>
</head>
<body style=""font-family: Arial, sans-serif; color: #333; text-align: center; padding: 20px;"">
    <div style=""max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 10px; padding: 20px; background-color: #f9f9f9;"">
        <h2 style=""color: #0066cc;"">تفعيل الحساب</h2>
        <p style=""font-size: 16px;"">لقد تلقينا طلبًا لتفعيل حسابك في <strong>Takles Tech</strong>.</p>
        <p style=""font-size: 16px;"">يرجى إدخال الكود التالي لتفعيل حسابك:</p>
        <p style=""display: inline-block; padding: 12px 25px; background-color: #007BFF; color: white; text-decoration: none; font-size: 16px; border-radius: 5px; margin-top: 10px;"">
            رمز التحقق: {0}
        </p>
        <p style=""font-size: 16px;"">يرجى إدخال هذا الكود في الصفحة الخاصة بتفعيل الحساب على موقعنا.</p>
        <p style=""font-size: 14px; color: #777; margin-top: 20px;"">إذا لم تطلب تفعيل الحساب، يرجى تجاهل هذه الرسالة.</p>
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
                await _emailService.SendEmailAsync(user.Email!, "تأكيد بريدك الإلكتروني", Body);
                var generatedToken = await _tokenVerifyService.GenerateToken(user);

                Response.Cookies.Append("token", generatedToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    Domain = ".takhleesak.com",
                    Path = "/"
                });


                return Ok(new ApiResponse { Message = "تم تسجيل حساب الأعمال بنجاح", Data = "VerifyCompanyEmail" });
            }

            return BadRequest(new ApiResponse { Message = "فشل في التسجيل" });
        }

        //انشاء حساب للمخلصين
        [HttpPost("Register-Broker")]
        public async Task<IActionResult> RegisterBroker([FromBody] BrokerDTO brokerDTO)
        {

            var existingUser = await _userManager.Users
                .AnyAsync(u => u.Email == brokerDTO.Email || u.PhoneNumber == brokerDTO.phoneNumber);

            if (existingUser)
            {
                return BadRequest(new ApiResponse { Message = "الرقم او البريد مستخدم بالفعل" });
            }

            if (brokerDTO.Password != brokerDTO.Confirm)
            {
                return BadRequest(new ApiResponse { Message = "كلمة المرور وتأكيد كلمة المرور غير متطابقتين" });
            }

            if (string.IsNullOrEmpty(brokerDTO.taxRecord) || string.IsNullOrEmpty(brokerDTO.InsuranceNumber))
            {
                return BadRequest(new ApiResponse { Message = "يجب تعبئة جميع الحقول المطلوبة" });
            }

            var user = new User
            {
                fullName = brokerDTO.fullName,
                Email = brokerDTO.Email,
                PhoneNumber = brokerDTO.phoneNumber,
                UserName = Guid.NewGuid().ToString(),
                Identity = brokerDTO.Identity,
                taxRecord = brokerDTO.taxRecord,
                InsuranceNumber = brokerDTO.InsuranceNumber,
                license = brokerDTO.license,
            };

            var result = await _userManager.CreateAsync(user, brokerDTO.Password!);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Broker");
                var verifyCode = await new Functions(_userManager, _db, _httpContextAccessor, _httpClient).GenerateVerifyCode(user, "VerifyBrokerEmail")!;
                var Body = string.Format(@"
<!DOCTYPE html>
<html lang=""ar"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>تفعيل الحساب</title>
</head>
<body style=""font-family: Arial, sans-serif; color: #333; text-align: center; padding: 20px;"">
    <div style=""max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 10px; padding: 20px; background-color: #f9f9f9;"">
        <h2 style=""color: #0066cc;"">تفعيل الحساب</h2>
        <p style=""font-size: 16px;"">لقد تلقينا طلبًا لتفعيل حسابك في <strong>Takles Tech</strong>.</p>
        <p style=""font-size: 16px;"">يرجى إدخال الكود التالي لتفعيل حسابك:</p>
        <div style=""display: inline-block; padding: 12px 25px; background-color: #007BFF; color: white; text-decoration: none; font-size: 16px; border-radius: 5px; margin-top: 10px; font-weight: bold;"">
            {0}
        </div>
        <p style=""font-size: 16px;"">يرجى إدخال هذا الكود في الصفحة الخاصة بتفعيل الحساب على موقعنا.</p>
        <p style=""font-size: 14px; color: #777; margin-top: 20px;"">إذا لم تطلب تفعيل الحساب، يرجى تجاهل هذه الرسالة.</p>
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
                await _emailService.SendEmailAsync(user.Email!, "تأكيد بريدك الإلكتروني", Body);
                var generatedToken = await _tokenVerifyService.GenerateToken(user);

                Response.Cookies.Append("token", generatedToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    Domain = ".takhleesak.com",
                    Path = "/"
                });
                return Ok(new ApiResponse { Message = "تم تسجيل حساب المخلصين بنجاح", Data = "VerifyBrokerEmail" });
            }

            return BadRequest(new ApiResponse { Message = "فشل في التسجيل" });
        }
    }
}
