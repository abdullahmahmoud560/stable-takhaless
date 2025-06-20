using User.Model;

namespace User.DTO
{
    public class GetOrdersDTO
    {
        public string? Location { get; set; }
        public string? typeOrder { get; set; }
        public string? statuOrder { get; set; }
        public string? Date { get; set; }
        public string? phoneNumber { get; set; }
        public string? fullName { get; set; }
        public string? Notes { get; set; }
        public string? BrokerID { get; set; }
        public double? Value { get; set; }
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? BrokerName { get; set; }
        public string? BrokerEmail { get; set; }
        public string? CustomerServiceName { get; set; }
        public string? CustomerServiceEmail { get; set; }
         public string? AccountName { get; set; }
        public string? AccountEmail { get; set; }

    }
}
