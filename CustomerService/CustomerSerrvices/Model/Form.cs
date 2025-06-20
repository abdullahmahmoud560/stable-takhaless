using System.ComponentModel.DataAnnotations;

namespace CustomerSerrvices.Models
{
    public class Form
    {
        public int Id { get; set; }
        public string? Message { get; set; }
        public string? Email { get; set; }
        public string? fullName { get; set; }
        public string? phoneNumber { get; set; }
        public DateTime createdAt { get; set; }
    }
}
