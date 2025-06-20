using System.ComponentModel.DataAnnotations;

namespace firstProject.DTO
{
    public class GetInformationDTO
    {
        [EmailAddress]
        public string? Email {  get; set; }
    }
}
