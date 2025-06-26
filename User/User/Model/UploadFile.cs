using System.ComponentModel.DataAnnotations;

namespace User.Model
{
    public class UploadFile
    {

        public int Id { get; set; }
        [Required]
        public string? fileName { get; set; }
        [Required]
        public string? fileUrl { get; set; }
        [Required]
        public string? ContentType { get; set; }
        public NewOrder? newOrder { get; set; }
        [Required]
        public int? newOrderId { get; set; }
    }
}
