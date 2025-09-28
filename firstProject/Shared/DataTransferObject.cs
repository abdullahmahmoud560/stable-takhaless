using System.ComponentModel.DataAnnotations;

namespace Shared
{
    public class DataTransferObject
    {
        public class ActiveUsersDTO
        {
            public string? fullName { get; set; }
            public string? Email { get; set; }
            public string? lastlogin { get; set; }
            public int? totalOrders { get; set; }
            public int? SuccessOrders { get; set; }
        }
        public class ApiResponse
        {
            public string? Message { get; set; }
            public Object? Data { get; set; }
            public string? State { get; set; }
        }
        public class BlockedDTO
        {
            [EmailAddress]
            public string? Email { get; set; }
        }
        public class BrokerDTO : CompanyDTO
        {
            public string? license { get; set; }
        }
        public class ChangeRolesDTO
        {
            public string? ID { get; set; }
            [AllowedValues("Admin", "User", "Manager", "CustomerService", "Broker", "Company", "Account")]
            public string? roleName { get; set; }
        }
        public class CompanyDTO : UserDTO
        {
            public string? taxRecord { get; set; }

            [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = ("ادخل الرقم التأميني بطريقة صحيحة "))]
            public string? InsuranceNumber { get; set; }
        }
        public class ErrorDetails
        {
            public int Status { get; set; }
            public string? Message { get; set; }
            public string? Details { get; set; }
        }
        public class ForgetPasswordDTO
        {
            [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = ("البريد الإلكترونى غير صحيح"))]
            public string? Email { get; set; }
        }
        public class GetInformationDTO
        {
            [EmailAddress]
            public string? Email { get; set; }
        }
        public class LoginDTO
        {
            [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = ("البريد الإلكترونى غير صحيح"))]
            public string? Email { get; set; }
            public string? Password { get; set; }

            //  [AllowedValues(true, false)]
            //  public bool RememberMe { get; set; }

        }
        public class ResetPasswordDTO
        {
            public string? newPassword { get; set; }           
            public string? Confirm { get; set; }
        }
        public class RolesDTO
        {
            public string? ID { get; set; }
            public List<string>? NameOfPermissions { get; set; }

        }
        public class SelectDTO
        {
            public int Id { get; set; }
            public string? fullName { get; set; }
            public string? Identity { get; set; }
            public string? taxRecord { get; set; }
            public string? InsuranceNumber { get; set; }
            public string? license { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Email { get; set; }
            public string? Role { get; set; }
        }
        public class UserDTO
        {
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$", ErrorMessage = ("ادخل كلمة المرور بطريقة صحيحة"))]
            public string Password { get; set; } = string.Empty;

            [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s]+$", ErrorMessage = ("يجب ان تحتوى على حروف فقط "))]
            [StringLength(45, MinimumLength = 3, ErrorMessage = (" ,يجب ادخال الاسم لايزيد عن  45 حرف  ولا يقل عن 7 حروف"))]
            public string fullName { get; set; } = string.Empty;
            public string Confirm { get; set; } =string.Empty;
            [RegularExpression(@"^\d+$", ErrorMessage = ("يجب ان تحتوى على ارقام فقط "))]
            [StringLength(15, MinimumLength = 6, ErrorMessage = ("ادخل رقم الهوية بطريقة صحيحة"))]
            public string Identity { get; set; } = string.Empty;
            [Phone]
            public string PhoneNumber { get; set; } =string.Empty;
            public string? Role { get; set; }
        }
        public class UserListDTO:UserDTO 
        {
            public bool IsBlocked;
            public int Id { get; set; }
        }
        public class GetID
        {
            public string? ID { get; set; }
            public string? BrokerID { get; set; }
        }
        public class VerifyCode
        {
            public string? Code { get; set; }

            [AllowedValues("VerifyLogin","VerifyBrokerEmail","VerifyCompanyEmail","VerifyUserEmail", "ForgetPassword")]
            public string? typeOfGenerate { get; set; }
        }
        }
    }
