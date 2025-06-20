using System.ComponentModel.DataAnnotations;

public class UserDTO
{
    [Required(ErrorMessage = ("هذا الحقل مطلوب"))]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = ("البريد الإلكترونى غير صحيح"))]
    public string? Email { get; set; }

    [Required(ErrorMessage = ("هذا الحقل مطلوب "))]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$", ErrorMessage = ("ادخل كلمة المرور بطريقة صحيحة"))]
    public string? Password { get; set; }

    [Required(ErrorMessage = ("هذا الحقل مطلوب "))]
    [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s]+$", ErrorMessage = ("يجب ان تحتوى على حروف فقط "))]
    [StringLength(45, MinimumLength = 3, ErrorMessage = (" ,يجب ادخال الاسم لايزيد عن  45 حرف  ولا يقل عن 7 حروف"))]
    public string? fullName { get; set; }

    [Required(ErrorMessage = ("هذا الحقل مطلوب "))]
    public string? Confirm { get; set; }

    [Required(ErrorMessage = ("هذا الحقل مطلوب "))]
    [RegularExpression(@"^\d+$", ErrorMessage = ("يجب ان تحتوى على ارقام فقط "))]
    [StringLength(15, MinimumLength = 6, ErrorMessage = ("ادخل رقم الهوية بطريقة صحيحة"))]
    public string? Identity { get; set; }

    [Required(ErrorMessage = ("هذا الحقل مطلوب "))]
    [RegularExpression(@"^\+?[0-9]{1,4}[-.\s]?[0-9]{1,4}[-.\s]?[0-9]{1,4}[-.\s]?[0-9]{1,4}$", ErrorMessage = "من فضلك ادخل رقم الهاتف صحيح.")]
    public string? phoneNumber { get; set; }

    public string? Role { get; set; }
    public bool? IsBlocked { get; set; }
    public string? Id { get; set; }
}