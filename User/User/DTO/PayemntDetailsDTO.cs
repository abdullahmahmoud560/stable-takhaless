namespace User.DTO
{
    public class PayemntDetailsDTO
    {
        public string? OrderId { get; set; }
        public string? UserId { get; set; }
        public string? Status { get; set; }
        public decimal? Amount { get; set; }
        public string? TransactionId { get; set; }
        public DateTime DateTime { get; set; }

    }
}
