using System.ComponentModel.DataAnnotations.Schema;

namespace freelancer.Models
{
    public class ChatMessage
    {
        [Column("MessageId")]
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty; 
        public string ReceiverId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
