using System.ComponentModel.DataAnnotations;

namespace firstProject.DTO
{
    public class ForgetPasswordDTO
    {
        [Required(ErrorMessage = ("هذا الحقل مطلوب"))]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = ("البريد الإلكترونى غير صحيح"))]
        public string? Email { get; set; }
    }
}
