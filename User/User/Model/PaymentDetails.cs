namespace User.Model
{
    public class PaymentDetails
    {
        public int Id { get; set; }
        public string? OrderId { get; set; }
        public string? UserId { get; set; }
        public string? Status { get; set; }
        public decimal? Amount { get; set; }
        public DateTime DateTime { get; set; }
        public string? TransactionId { get; set; }
    }
}
