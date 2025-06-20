namespace User.Model
{
    public class NotesAccounting
    {
        public int Id { get; set; }
        public string? Notes { get; set; }
        public string? UserID { get; set; }
        public NewOrder? newOrder { get; set; }
        public int? newOrderId { get; set; }
    }
}
