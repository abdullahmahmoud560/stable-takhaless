using System.ComponentModel.DataAnnotations;

namespace firstProject.DTO
{
    public class VerifyCode
    {
        [Required]
        public string? Code { get; set; }
        public string? typeOfGenerate { get; set; }
    }
}
