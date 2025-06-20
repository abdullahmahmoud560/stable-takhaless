using System.ComponentModel.DataAnnotations;

namespace User.DTO
{
    public class GetID
    {
        [RegularExpression(@"^-?[0-9]+$")]
        public int ID { get; set; }
        public string? statuOrder { get; set; }
        public string? BrokerID { get; set; }
        public string? Notes { get; set;}
        public double? Value { get; set; }
    }
}
