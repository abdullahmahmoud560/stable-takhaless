namespace Admin.DTO
{
    public class AddLogs
    {
        public string? Message { get; set; }
        public int? NewOrderId { get; set; }
        public DateTime? TimeStamp { get; set; } = DateTime.Now;
        public string? UserId { get; set; }
        public string? Notes { get; set; }
    }
}
