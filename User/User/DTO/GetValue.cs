using System.ComponentModel.DataAnnotations;

namespace User.DTO
{
    public class GetValue
    {
        [RegularExpression(@"^[0-9٠-٩]+([.,٫][0-9٠-٩]+)?$")]
        public double Value { get; set; }
        public int? newOrderId { get; set; }
        public int? Count { get; set; }
        public string? BrokerID { get; set; }
    }
}
