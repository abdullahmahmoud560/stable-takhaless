namespace freelancer.Models
{
    public class ChatSummary
    {
        public int Id { get; set; }

        public string User1Id { get; set; } =string.Empty;
        public string User2Id { get; set; } =string.Empty;
        public string User1Name { get; set; }= string.Empty;
        public string User2Name { get; set; }=  string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageTime { get; set; }
    }
}
