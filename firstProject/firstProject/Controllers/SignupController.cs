using Application.Interface;
using AutoMapper;
using Infrastructure.Services;
using Infrastructure.Validation;
using Microsoft.AspNetCore.Mvc;
using static Shared.DataTransferObject;

namespace firstProject.Controllers
{
    [Route("api/")]
    [ApiController]

    public class SignupController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public SignupController(IServiceManager serviceManager,IUserService userService,IMapper mapper)
        {
            _serviceManager = serviceManager;
            _userService = userService;
            _mapper = mapper;
        }

        [HttpPost("Register-user")]
        public async Task<IActionResult> Register_user([FromBody] UserDTO registerDTO)
        {
            if (string.IsNullOrWhiteSpace(registerDTO.Email) || string.IsNullOrEmpty(registerDTO.PhoneNumber)
                || string.IsNullOrEmpty(registerDTO.fullName) || string.IsNullOrEmpty(registerDTO.Identity)
                || string.IsNullOrEmpty(registerDTO.Password) || string.IsNullOrEmpty(registerDTO.Confirm))
            {
                return BadRequest(new ApiResponse { Message = "برجاء ملئ جميع الحقول" });
            }
            registerDTO.Email = InputSanitizer.SanitizeEmail(registerDTO.Email);
            registerDTO.fullName = InputSanitizer.SanitizeName(registerDTO.fullName);
            registerDTO.PhoneNumber = InputSanitizer.SanitizePhoneNumber(registerDTO.PhoneNumber);
            registerDTO.Identity = InputSanitizer.SanitizeIdentity(registerDTO.Identity);

            var isFound = await _userService.CheckIsFoundEmailOrPhoneOrIdentity(registerDTO.Email,registerDTO.PhoneNumber,registerDTO.Identity);

            if (!isFound.Success)
            {
                return BadRequest(new ApiResponse { Message = isFound.Error });
            }
            if (registerDTO.Password != registerDTO.Confirm)
            {
                return BadRequest(new ApiResponse { Message = "كلمة المرور وتأكيد كلمة المرور غير متطابقتين" });
            }
            registerDTO.Role = "User";
            var user = _mapper.Map<BrokerDTO>(registerDTO);

            var result = await _userService.CreateAsync(user, registerDTO.Password!);
            if (result.Success)
            {
                var verifyCode = await _serviceManager.FunctionService.GenerateVerifyCode(registerDTO.Email, "VerifyUserEmail")!;
                if (!verifyCode.All(c => char.IsDigit(c)))
                    return BadRequest(new ApiResponse{ Message = verifyCode });
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
                var send = await _serviceManager.EmailService.SendEmailAsync(user.Email!, "تأكيد بريدك الإلكتروني", Body);
                if (!send.Success)
                    return BadRequest(new ApiResponse { Message = send.Error });
                var generatedToken = await _serviceManager.TokenService.GenerateActiveToken(registerDTO.Email);
                if (!generatedToken.Success)
                    return BadRequest(new ApiResponse { Message = generatedToken.Error});

                CookieHelper.SetTokenCookieInDays(Response, generatedToken.Error, 14);
                return Ok(new ApiResponse { Message = "تم تسجيل حساب الافراد بنجاح", Data = "VerifyUserEmail" });
            }

            return BadRequest(new ApiResponse { Message = result.Error });
        }

        [HttpPost("Register-company")]
        public async Task<IActionResult> Register_company([FromBody] CompanyDTO companyDTO)
        {
            if (string.IsNullOrEmpty(companyDTO.Email) || string.IsNullOrEmpty(companyDTO.PhoneNumber)
            || string.IsNullOrEmpty(companyDTO.fullName) || string.IsNullOrEmpty(companyDTO.Identity)
            || string.IsNullOrEmpty(companyDTO.Password) || string.IsNullOrEmpty(companyDTO.Confirm)
            || string.IsNullOrEmpty(companyDTO.InsuranceNumber) || string.IsNullOrEmpty(companyDTO.taxRecord))
            {
                return BadRequest(new ApiResponse { Message = "برجاءملئ جميع الحقول" });
            }
            
            // Input Sanitization للحقول الحساسة
            companyDTO.Email = InputSanitizer.SanitizeEmail(companyDTO.Email);
            companyDTO.fullName = InputSanitizer.SanitizeName(companyDTO.fullName);
            companyDTO.PhoneNumber = InputSanitizer.SanitizePhoneNumber(companyDTO.PhoneNumber);
            companyDTO.Identity = InputSanitizer.SanitizeIdentity(companyDTO.Identity);
            companyDTO.taxRecord = InputSanitizer.SanitizeString(companyDTO.taxRecord);
            companyDTO.InsuranceNumber = InputSanitizer.SanitizeString(companyDTO.InsuranceNumber);
            var isFound = await _userService.CheckIsFoundEmailOrPhoneOrIdentity(companyDTO.Email, companyDTO.PhoneNumber,companyDTO.Identity);

            if (!isFound.Success)
            {
                return BadRequest(new ApiResponse { Message = isFound.Error });
            }

            var isFound2 = await _userService.CheckIsFoundtaxRecordOrInsuranceNumber(companyDTO.InsuranceNumber, companyDTO.taxRecord,string.Empty);

            if (!isFound2.Success)
            {
                return BadRequest(new ApiResponse { Message = isFound2.Error });
            }

            if (companyDTO.Password != companyDTO.Confirm)
            {
                return BadRequest(new ApiResponse { Message = "كلمة المرور وتأكيد كلمة المرور غير متطابقتين" });
            }
            companyDTO.Role = "Company";
            var company = _mapper.Map<BrokerDTO>(companyDTO);
            var result = await _userService.CreateAsync(company, companyDTO.Password!);
            if (result.Success)
            {
                var verifyCode = await _serviceManager.FunctionService.GenerateVerifyCode(companyDTO.Email, "VerifyCompanyEmail")!;
                if (!verifyCode.All(c => char.IsDigit(c)))
                    return BadRequest(new ApiResponse{ Message = verifyCode });
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
                var send = await _serviceManager.EmailService.SendEmailAsync(companyDTO.Email!, "تأكيد بريدك الإلكتروني", Body);
                if (!send.Success)
                    return BadRequest(new ApiResponse { Message = send.Error });
                var generatedToken = await _serviceManager.TokenService.GenerateActiveToken(companyDTO.Email);
                if (!generatedToken.Success)
                    return BadRequest(new ApiResponse { Message = generatedToken.Error });
                CookieHelper.SetTokenCookieInDays(Response, generatedToken.Error, 14);

                return Ok(new ApiResponse { Message = "تم تسجيل حساب الأعمال بنجاح", Data = "VerifyCompanyEmail" });
            }

            return BadRequest(new ApiResponse { Message = "فشل في التسجيل" });
        }

        [HttpPost("Register-Broker")]
        public async Task<IActionResult> RegisterBroker([FromBody] BrokerDTO brokerDTO)
        {
            if (string.IsNullOrEmpty(brokerDTO.Email) || string.IsNullOrEmpty(brokerDTO.PhoneNumber)
            || string.IsNullOrEmpty(brokerDTO.fullName) || string.IsNullOrEmpty(brokerDTO.Identity)
            || string.IsNullOrEmpty(brokerDTO.Password) || string.IsNullOrEmpty(brokerDTO.Confirm)
            || string.IsNullOrEmpty(brokerDTO.InsuranceNumber) || string.IsNullOrEmpty(brokerDTO.taxRecord)
            || string.IsNullOrEmpty(brokerDTO.license))
            {
                return BadRequest(new ApiResponse { Message = "برجاءملئ جميع الحقول" });
            }
            
            // Input Sanitization للحقول الحساسة
            brokerDTO.Email = InputSanitizer.SanitizeEmail(brokerDTO.Email);
            brokerDTO.fullName = InputSanitizer.SanitizeName(brokerDTO.fullName);
            brokerDTO.PhoneNumber = InputSanitizer.SanitizePhoneNumber(brokerDTO.PhoneNumber);
            brokerDTO.Identity = InputSanitizer.SanitizeIdentity(brokerDTO.Identity);
            brokerDTO.taxRecord = InputSanitizer.SanitizeString(brokerDTO.taxRecord);
            brokerDTO.InsuranceNumber = InputSanitizer.SanitizeString(brokerDTO.InsuranceNumber);
            brokerDTO.license = InputSanitizer.SanitizeString(brokerDTO.license);

            var isFound = await _userService.CheckIsFoundEmailOrPhoneOrIdentity(brokerDTO.Email, brokerDTO.PhoneNumber,brokerDTO.Identity);

            if (!isFound.Success)
            {
                return BadRequest(new ApiResponse { Message = isFound.Error });
            }

            var isFound2 = await _userService.CheckIsFoundtaxRecordOrInsuranceNumber(brokerDTO.Email, brokerDTO.PhoneNumber,brokerDTO.license);

            if (!isFound2.Success)
            {
                return BadRequest(new ApiResponse { Message = isFound2.Error });
            }

            if (brokerDTO.Password != brokerDTO.Confirm)
            {
                return BadRequest(new ApiResponse { Message = "كلمة المرور وتأكيد كلمة المرور غير متطابقتين" });
            }

            brokerDTO.Role = "Broker";
            var result = await _userService.CreateAsync(brokerDTO, brokerDTO.Password!);
            if (result.Success)
            {
                var verifyCode = await _serviceManager.FunctionService.GenerateVerifyCode(brokerDTO.Email, "VerifyBrokerEmail")!;
                if (!verifyCode.All(c => char.IsDigit(c)))
                    return BadRequest(new { Message = verifyCode });
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
                var send = await _serviceManager.EmailService.SendEmailAsync(brokerDTO.Email!, "تأكيد بريدك الإلكتروني", Body);
                if (!send.Success)
                    return BadRequest(new ApiResponse { Message = send.Error });
                var generatedToken = await _serviceManager.TokenService.GenerateActiveToken(brokerDTO.Email);
                if (!generatedToken.Success)
                    return BadRequest(new ApiResponse { Message = generatedToken.Error});
                CookieHelper.SetTokenCookieInDays(Response, generatedToken.Error, 14);
                return Ok(new ApiResponse { Message = "تم تسجيل حساب المخلصين بنجاح", Data = "VerifyBrokerEmail" });
            }
            return BadRequest(new ApiResponse { Message = "فشل في التسجيل" });
        }
    }
}
