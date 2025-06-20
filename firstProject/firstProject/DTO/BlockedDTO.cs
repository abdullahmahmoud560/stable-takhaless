using System.ComponentModel.DataAnnotations;

namespace firstProject.DTO
{
    public class BlockedDTO
    {
        [EmailAddress]
        public string? Email { get; set; }
    }
}
