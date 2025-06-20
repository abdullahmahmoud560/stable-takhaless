using System.ComponentModel.DataAnnotations;

namespace firstProject.DTO
{
    public class ResetPasswordDTO
    {
        [Required(ErrorMessage = ("هذا الحقل مطلوب "))]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$", ErrorMessage = ("ادخل كلمة المرور بطريقة صحيحة"))]
        public string? newPassword { get; set; }
        [Required(ErrorMessage = ("هذا الحقل مطلوب "))]
        [Compare("newPassword", ErrorMessage = ("كلمة المرور غير متطابقة "))]
        public string? Confirm { get; set; }

        public string? Code { get; set; }

    }
}
