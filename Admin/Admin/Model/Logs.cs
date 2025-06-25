namespace Admin.Model
{
    public class Logs
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public int NewOrderId { get; set; }
        public DateTime? TimeStamp { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
