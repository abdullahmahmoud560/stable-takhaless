using System.ComponentModel.DataAnnotations;

namespace firstProject.DTO
{
    public class SMSDTO
    {
        [Required(ErrorMessage = ("هذا الحقل مطلوب "))]
        [RegularExpression(@"^\+?[0-9]{1,4}[-.\s]?[0-9]{1,4}[-.\s]?[0-9]{1,4}[-.\s]?[0-9]{1,4}$", ErrorMessage = "من فضلك ادخل رقم الهاتف صحيح.")]
        public string? To { get; set; }

        [StringLength(8,MinimumLength =4)]
        [Required(ErrorMessage = ("هذا الحقل مطلوب "))]
        [RegularExpression(@"^\d+$", ErrorMessage = ("البيانات غير صحيحة "))]
        public string? Text { get; set; }
    }
}
