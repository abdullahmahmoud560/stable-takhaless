using System.ComponentModel.DataAnnotations;
using CustomerSerrvices.Models;

namespace CustomerSerrvices.DTO
{
    public class FormDTO
    {
        [Required]
        public string? Message { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = ("البريد الإلكترونى غير صحيح"))]
        public string? Email { get; set; }
        [Required]
        [StringLength(45, MinimumLength = 3, ErrorMessage = (" ,يجب ادخال الاسم لايزيد عن  45 حرف  ولا يقل عن 7 حروف"))]
        [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s]+$")]
        public string? fullName { get; set; }

        [Required]
        [RegularExpression(@"^\+?[0-9]{1,4}[-.\s]?[0-9]{1,4}[-.\s]?[0-9]{1,4}[-.\s]?[0-9]{1,4}$")]
        public string? phoneNumber { get; set; }
    }
}
