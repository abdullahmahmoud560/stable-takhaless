using System.ComponentModel.DataAnnotations;

namespace firstProject.DTO
{
    public class ChageRolesDTO
    {
        [Required]
        public string? ID { get; set; }
        [Required]

        [AllowedValues("Admin","User","Manager","CustomerService","Broker","Company","Account")]
        public string? roleName { get; set; }
    }
}
