using System.ComponentModel.DataAnnotations;

namespace firstProject.DTO
{
    public class CompanyDTO :UserDTO
    {

        [Required(ErrorMessage = ("هذا الحقل مطلوب "))]
        public string? taxRecord { get; set; }

        [Required(ErrorMessage = ("هذا الحقل مطلوب"))]
        [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = ("ادخل الرقم التأميني بطريقة صحيحة "))]
        public string? InsuranceNumber { get; set; }
    }
}
