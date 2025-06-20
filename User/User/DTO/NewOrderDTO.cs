using System.ComponentModel.DataAnnotations;
using User.Model;

namespace User.DTO
{
    public class NewOrderDTO
    {
        [RegularExpression(@"^[\p{L}\s]+$")]
       public string? Location { get; set; }
        [RegularExpression(@"^[0-9\u0660-\u0669]+$")]
        public string? numberOfLicense { get; set; }
        public List<NumberOfOrdersDTO>? numberOfTypeOrders {  get; set; }
        public List<IFormFile>? uploadFile{ get; set; }
        public string? Notes { get; set; }
        public string? City { get; set; }
        public string? Town { get; set; }
        public string? zipCode { get; set; }
       
    }
}
