using System.ComponentModel.DataAnnotations;

namespace firstProject.DTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage = ("هذا الحقل مطلوب "))]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = ("البريد الإلكترونى غير صحيح"))]
        public string? Email { get; set; }

        [Required(ErrorMessage = ("هذا الحقل مطلوب "))]
        public string? Password { get; set; }

      //  [AllowedValues(true, false)]
      //  public bool RememberMe { get; set; }

    }
}
